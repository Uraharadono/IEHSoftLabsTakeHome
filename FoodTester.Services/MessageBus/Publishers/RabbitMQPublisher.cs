using FoodTester.Infrastructure.MessageBus.Messages;
using FoodTester.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodTester.Services.MessageBus.Publishers
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string ExchangeName = "food_analysis";
        private const string QueueName = "analysis_requests";
        private const string RoutingKey = "analysis.request";
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMQPublisher(IConfiguration configuration, IOptions<AppSettings> settings)
        {
            _rabbitMqSettings = settings.Value.RabbitMqSettings;
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSettings.HostName,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange and queue
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
        }

        public Task PublishAnalysisRequestAsync(FoodAnalysisMessage message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;  // Message persistence
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
