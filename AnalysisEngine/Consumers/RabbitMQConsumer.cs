using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using FoodTester.Infrastructure.MessageBus.Messages;
using AnalysisEngine.DockerService;
using FoodTester.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Grpc.Net.Client;
using FoodTester.Infrastructure.Grpc;

namespace AnalysisEngine.Consumers
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IDockerService _dockerService;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly Dictionary<string, GrpcChannel> _grpcChannels;
        private const string ExchangeName = "food_analysis";
        private const string QueueName = "analysis_requests";
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger, IOptions<AppSettings> settings, IDockerService dockerService)
        {
            _logger = logger;
            _rabbitMqSettings = settings.Value.RabbitMqSettings;
            _dockerService = dockerService;
            _grpcChannels = new Dictionary<string, GrpcChannel>();

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSettings.HostName,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the same exchange and queue as in publisher
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<FoodAnalysisMessage>(Encoding.UTF8.GetString(body));

                try
                {
                    await ProcessMessage(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: QueueName,
                                  autoAck: false,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessMessage(FoodAnalysisMessage message)
        {
            string containerId = null;
            GrpcChannel grpcChannel = null;

            try
            {
                _logger.LogInformation("Starting analysis process for serial number: {SerialNumber}",
                    message.SerialNumber);

                // 1. Create and start the AnalysisWorker container
                containerId = await _dockerService.StartAnalysisWorkerAsync(message);
                _logger.LogInformation("Started container {ContainerId}", containerId);

                // 2. Get the container's gRPC port mapping
                var containerInfo = await _dockerService.GetContainerInfoAsync(containerId);
                var hostPort = containerInfo.Ports.First().PublicPort;

                // 3. Create gRPC channel to the worker
                var channelUrl = $"http://localhost:{hostPort}";
                grpcChannel = GrpcChannel.ForAddress(channelUrl);
                _grpcChannels[containerId] = grpcChannel;

                var client = new AnalysisService.AnalysisServiceClient(grpcChannel);

                // 4. Start the analysis via gRPC
                var analysisRequest = new AnalysisRequest
                {
                    SerialNumber = message.SerialNumber,
                    FoodType = message.FoodType
                };
                analysisRequest.RequiredAnalyses.AddRange(message.RequiredAnalyses);

                var startResponse = await client.StartAnalysisAsync(analysisRequest);

                if (!startResponse.Success)
                {
                    throw new Exception($"Failed to start analysis: {startResponse.Message}");
                }

                // 5. Poll for analysis completion
                var analysisComplete = false;
                var statusRequest = new StatusRequest { AnalysisId = startResponse.AnalysisId };
                var results = new List<AnalysisResult>();

                while (!analysisComplete)
                {
                    var statusResponse = await client.GetAnalysisStatusAsync(statusRequest);

                    switch (statusResponse.Status)
                    {
                        case AnalysisStatus.Completed:
                            analysisComplete = true;
                            results.AddRange(statusResponse.Results);
                            break;

                        case AnalysisStatus.Failed:
                            throw new Exception($"Analysis failed: {statusResponse.ErrorMessage}");

                        case AnalysisStatus.InProgress:
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            break;

                        default:
                            throw new Exception($"Unknown analysis status: {statusResponse.Status}");
                    }
                }

                // 6. Process the results and send them back to QualityManager
                await PublishResultsToQualityManager(message.SerialNumber, results);
            }
            finally
            {
                // 7. Cleanup: Close gRPC channel and stop container
                if (grpcChannel != null)
                {
                    _grpcChannels.Remove(containerId);
                    await grpcChannel.ShutdownAsync();
                }

                if (containerId != null)
                {
                    await _dockerService.StopContainerAsync(containerId);
                    _logger.LogInformation("Stopped and removed container {ContainerId}", containerId);
                }
            }
        }

        private async Task PublishResultsToQualityManager(string serialNumber, List<AnalysisResult> results)
        {
            // Convert results to your message format and publish to QualityManager
            var resultMessage = new AnalysisResultMessage
            {
                SerialNumber = serialNumber,
                CompletedAt = DateTime.UtcNow,
                Results = results.Select(r => new AnalysisResultDetail
                {
                    AnalysisType = r.AnalysisType,
                    Passed = r.Passed,
                    Value = r.Value,
                    Unit = r.Unit,
                    Details = r.Details
                }).ToList()
            };

            var messageBody = JsonSerializer.SerializeToUtf8Bytes(resultMessage);
            _channel.BasicPublish(
                exchange: "food_analysis",
                routingKey: "analysis_results",
                basicProperties: null,
                body: messageBody);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Cleanup any remaining channels and containers
            foreach (var (containerId, channel) in _grpcChannels)
            {
                await channel.ShutdownAsync();
                await _dockerService.StopContainerAsync(containerId);
            }
            _grpcChannels.Clear();

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
