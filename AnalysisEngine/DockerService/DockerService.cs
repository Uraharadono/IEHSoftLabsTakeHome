using Docker.DotNet;
using Docker.DotNet.Models;
using FoodTester.Infrastructure.MessageBus.Messages;
using System.Text;

namespace AnalysisEngine.DockerService
{
    public class DockerService : IDockerService
    {
        private readonly DockerClient _dockerClient;
        private readonly ILogger<DockerService> _logger;
        private const string WorkerImageName = "analysis-worker:latest";

        public DockerService(ILogger<DockerService> logger)
        {
            _logger = logger;

            // For Windows, use named pipe or TCP
            var isWindows = System.Runtime.InteropServices.RuntimeInformation
                .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

            if (isWindows)
            {
                // Try named pipe first (default Docker Desktop installation)
                _dockerClient = new DockerClientConfiguration(
                    new Uri("npipe://./pipe/docker_engine"))
                        .CreateClient();
            }
            else
            {
                // Unix socket for Linux
                _dockerClient = new DockerClientConfiguration(
                    new Uri("unix:///var/run/docker.sock"))
                        .CreateClient();
            }
        }

        public async Task<string> StartAnalysisWorkerAsync(FoodAnalysisMessage message)
        {
            try
            {
                // First, check if the image exists
                try
                {
                    await _dockerClient.Images.InspectImageAsync(WorkerImageName);
                }
                catch (DockerImageNotFoundException)
                {
                    _logger.LogError($"Image {WorkerImageName} not found. Please build the analysis-worker image first.");
                    throw new InvalidOperationException($"Docker image {WorkerImageName} not found");
                }

                // Create container with more detailed logging
                _logger.LogInformation("Creating container for analysis worker...");

                var createParams = new CreateContainerParameters
                {
                    Image = WorkerImageName,
                    Env = new[]
                    {
                        $"SERIAL_NUMBER={message.SerialNumber}",
                        $"FOOD_TYPE={message.FoodType}",
                        $"REQUIRED_ANALYSES={string.Join(",", message.RequiredAnalyses ?? Array.Empty<string>())}"
                    },
                    HostConfig = new HostConfig
                    {
                        AutoRemove = true,
                        Memory = 512L * 1024L * 1024L,
                        MemorySwap = 512L * 1024L * 1024L,
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                "5000/tcp",
                                new List<PortBinding>
                                {
                                    new PortBinding
                                    {
                                        HostPort = "0" // Docker will assign a random available port
                                    }
                                }
                            }
                        }
                    },
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        { "5000/tcp", new EmptyStruct() }
                    }
                };

                var container = await _dockerClient.Containers.CreateContainerAsync(createParams);
                _logger.LogInformation($"Container created with ID: {container.ID}");

                // Start container
                await _dockerClient.Containers.StartContainerAsync(
                    container.ID,
                    new ContainerStartParameters());

                // Wait for container to be ready
                const int maxRetries = 10;
                const int delayMs = 500;
                var success = false;

                for (int i = 0; i < maxRetries && !success; i++)
                {
                    try
                    {
                        await Task.Delay(delayMs); // Wait before checking

                        var containers = await _dockerClient.Containers.ListContainersAsync(
                            new ContainersListParameters
                            {
                                All = true,
                                Filters = new Dictionary<string, IDictionary<string, bool>>
                                {
                            {
                                "id",
                                new Dictionary<string, bool>
                                {
                                    { container.ID, true }
                                }
                            }
                                }
                            });

                        if (containers.Any(c => c.ID == container.ID))
                        {
                            success = true;
                            _logger.LogInformation(
                                "Successfully verified container {ContainerId} is running for serial number {SerialNumber}",
                                container.ID,
                                message.SerialNumber);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Container {ContainerId} not found in running containers list, attempt {Attempt}/{MaxRetries}",
                                container.ID,
                                i + 1,
                                maxRetries);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Error checking container status on attempt {Attempt}/{MaxRetries}",
                            i + 1,
                            maxRetries);

                        if (i == maxRetries - 1)
                            throw;
                    }
                }

                if (!success)
                {
                    throw new Exception($"Container {container.ID} failed to start properly after {maxRetries} attempts");
                }

                return container.ID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error starting analysis worker for serial number {SerialNumber}. Error: {Error}",
                    message.SerialNumber,
                    ex.Message);
                throw;
            }
        }

        public async Task StopContainerAsync(string containerId)
        {
            try
            {
                await _dockerClient.Containers.StopContainerAsync(
                    containerId,
                    new ContainerStopParameters { WaitBeforeKillSeconds = 30 });

                _logger.LogInformation("Stopped container {ContainerId}", containerId);
            }
            catch (DockerContainerNotFoundException)
            {
                _logger.LogWarning("Container {ContainerId} not found - may have already been removed", containerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping container {ContainerId}", containerId);
                throw;
            }
        }

        public async Task<string> GetContainerLogsAsync(string containerId)
        {
            try
            {
                var logStream = await _dockerClient.Containers.GetContainerLogsAsync(
                    containerId,
                    false,
                    new ContainerLogsParameters
                    {
                        ShowStdout = true,
                        ShowStderr = true,
                        Timestamps = true
                    });

                return string.Join(";", (await ReadOutputAsync(logStream)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for container {ContainerId}", containerId);
                throw;
            }
        }

        private static async Task<List<string>> ReadOutputAsync(MultiplexedStream multiplexedStream, CancellationToken cancellationToken = default)
        {
            List<string> logs = new List<string>();
            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(81920);

            while (true)
            {
                Array.Clear(buffer, 0, buffer.Length);

                MultiplexedStream.ReadResult readResult = await multiplexedStream.ReadOutputAsync(buffer, 0, buffer.Length, cancellationToken);

                if (readResult.EOF)
                {
                    break;
                }

                if (readResult.Count > 0)
                {
                    var responseLine = Encoding.UTF8.GetString(buffer, 0, readResult.Count);

                    logs.Add(responseLine.Trim());
                }
                else
                {
                    break;
                }
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            return logs;
        }

        public async Task<ContainerInfo> GetContainerInfoAsync(string containerId)
        {
            const int maxRetries = 5;
            const int delayMilliseconds = 500;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _dockerClient.Containers.InspectContainerAsync(containerId);
                    var portBindings = response.NetworkSettings.Ports;

                    var portMappings = new List<PortMapping>();
                    foreach (var binding in portBindings)
                    {
                        if (binding.Value != null && binding.Value.Any())
                        {
                            portMappings.Add(new PortMapping
                            {
                                PrivatePort = int.Parse(binding.Key.Split('/')[0]),
                                PublicPort = int.Parse(binding.Value[0].HostPort),
                                Type = binding.Key.Split('/')[1]
                            });
                        }
                    }

                    return new ContainerInfo
                    {
                        Id = containerId,
                        Ports = portMappings
                    };
                }
                catch (DockerContainerNotFoundException) when (i < maxRetries - 1)
                {
                    _logger.LogWarning("Container {ContainerId} not found on attempt {Attempt}, retrying...",
                        containerId, i + 1);
                    await Task.Delay(delayMilliseconds);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting container info for {ContainerId}", containerId);
                    throw;
                }
            }

            throw new DockerContainerNotFoundException(System.Net.HttpStatusCode.InternalServerError, $"Container {containerId} not found after {maxRetries} attempts");
        }
    }
}
