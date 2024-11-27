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
using Grpc.Core;
using AnalysisEngine.Publishers;

namespace AnalysisEngine.Consumers
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IDockerService _dockerService;
        private readonly IResultPublisher _resultPublisher;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly Dictionary<string, GrpcChannel> _grpcChannels;
        private const string ExchangeName = "food_analysis";
        private const string QueueName = "analysis_requests";
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger, IOptions<AppSettings> settings, IDockerService dockerService, IResultPublisher resultPublisher)
        {
            _logger = logger;
            _rabbitMqSettings = settings.Value.RabbitMqSettings;
            _dockerService = dockerService;
            _grpcChannels = new Dictionary<string, GrpcChannel>();
            _resultPublisher = resultPublisher;

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
            _logger.LogInformation("Starting RabbitMQ consumer...");

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var messageBody = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received message: {MessageBody}", messageBody);

                    var message = JsonSerializer.Deserialize<FoodAnalysisMessage>(messageBody);

                    _logger.LogInformation("Deserialized message for serial number: {SerialNumber}", message.SerialNumber);

                    await ProcessMessage(message);
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing received message");
                    _channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: QueueName,
                                  autoAck: false,
                                  consumer: consumer);

            _logger.LogInformation("RabbitMQ consumer started and waiting for messages");

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
                var channelUrl = $"http://analysisworker:8080";
                grpcChannel = GrpcChannel.ForAddress(channelUrl);
                _grpcChannels[containerId] = grpcChannel;
                // var channelUrl = $"http://localhost:{hostPort}";
                /*var channelUrl = "https://localhost:44336";
                grpcChannel = GrpcChannel.ForAddress(channelUrl);*/

                var client = new AnalysisService.AnalysisServiceClient(grpcChannel);

                // 4. Start the analysis via gRPC
                var analysisRequest = new AnalysisRequest
                {
                    SerialNumber = message.SerialNumber,
                    FoodType = "FoodType",
                };
                analysisRequest.RequiredAnalyses.AddRange(message.RequiredAnalyses.Select(s => new FoodTester.Infrastructure.Grpc.FoodAnalysisType
                {
                    AnalysisId = s.AnalysisId,
                    AnalysisName = s.AnalysisName
                }).ToList());

                // 5. Process streaming responses
                var results = new List<AnalysisResult>();
                var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30)); // Timeout after 30 minutes

                using var call = client.AnalyzeFood(analysisRequest, cancellationToken: cts.Token);
                await foreach (var update in call.ResponseStream.ReadAllAsync(cts.Token))
                {
                    switch (update.Status)
                    {
                        case AnalysisStatus.InProgress:
                            if (update.Result != null)
                            {
                                results.Add(update.Result);
                                _logger.LogInformation("Received result for {AnalysisType}",
                                    update.Result.AnalysisType);
                            }
                            break;

                        case AnalysisStatus.Completed:
                            _logger.LogInformation("Analysis completed for {SerialNumber}", message.SerialNumber);
                            await PublishResultsToQualityManager(message.SerialNumber, results);
                            return;

                        case AnalysisStatus.Failed:
                            throw new Exception($"Analysis failed: {update.ErrorMessage}");

                        default:
                            _logger.LogWarning("Received unknown status: {Status}", update.Status);
                            break;
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
            try
            {
                var resultMessage = new AnalysisResultMessage
                {
                    SerialNumber = serialNumber,
                    CompletedAt = DateTime.UtcNow,
                    Results = results.Select(r => new AnalysisResultDetail
                    {
                        AnalysisId = r.AnalysisId,
                        AnalysisType = r.AnalysisType,
                        Passed = r.Passed,
                        Value = r.Value,
                        Unit = r.Unit,
                        Details = r.Details
                    }).ToList()
                };

                await _resultPublisher.PublishAnalysisResultsAsync(resultMessage);

                _logger.LogInformation("Successfully published results for serial number: {SerialNumber}", serialNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish results for serial number: {SerialNumber}", serialNumber);
                throw;
            }
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
