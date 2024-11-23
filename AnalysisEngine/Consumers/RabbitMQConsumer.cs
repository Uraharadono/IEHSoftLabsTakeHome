using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using FoodTester.Infrastructure.MessageBus.Messages;
using AnalysisEngine.DockerService;
using FoodTester.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace AnalysisEngine.Consumers
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IDockerService _dockerService;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private const string ExchangeName = "food_analysis";
        private const string QueueName = "analysis_requests";
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger, IOptions<AppSettings> settings, IDockerService dockerService)
        {
            _logger = logger;
            _rabbitMqSettings = settings.Value.RabbitMqSettings;
            _dockerService = dockerService;

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
            _logger.LogInformation("Processing analysis request for serial number: {SerialNumber}", message.SerialNumber);

            try
            {
                // Start analysis worker container
                var containerId = await _dockerService.StartAnalysisWorkerAsync(message);

                // Wait for a while to let the analysis complete
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Get container logs
                var logs = await _dockerService.GetContainerLogsAsync(containerId);

                // Stop the container
                await _dockerService.StopContainerAsync(containerId);

                // Here you would process the results and send them back to QualityManager
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing analysis for serial number {SerialNumber}",
                    message.SerialNumber);
                throw;
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
