using RabbitMQ.Client;
using System.Text.Json;
using FoodTester.Infrastructure.MessageBus.Messages;
using FoodTester.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace AnalysisEngine.Publishers
{
    public class RabbitMQResultPublisher : IResultPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string ExchangeName = "food_analysis";
        private const string RoutingKey = "analysis_results";
        private readonly ILogger<RabbitMQResultPublisher> _logger;

        public RabbitMQResultPublisher(IConfiguration configuration, IOptions<AppSettings> settings, ILogger<RabbitMQResultPublisher> logger)
        {
            _logger = logger;
            var rabbitMqSettings = settings.Value.RabbitMqSettings;
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.HostName,
                UserName = rabbitMqSettings.UserName,
                Password = rabbitMqSettings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange for results
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
        }

        public Task PublishAnalysisResultsAsync(AnalysisResultMessage resultMessage)
        {
            try
            {
                var messageBody = JsonSerializer.SerializeToUtf8Bytes(resultMessage);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    basicProperties: properties,
                    body: messageBody);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}