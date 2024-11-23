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
                        MemorySwap = 512L * 1024L * 1024L
                    }
                };

                var container = await _dockerClient.Containers.CreateContainerAsync(createParams);

                _logger.LogInformation($"Container created with ID: {container.ID}");

                // Start container
                await _dockerClient.Containers.StartContainerAsync(
                    container.ID,
                    new ContainerStartParameters());

                _logger.LogInformation(
                    "Started analysis worker container {ContainerId} for serial number {SerialNumber}",
                    container.ID,
                    message.SerialNumber);

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
    }
}
