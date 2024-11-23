using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using FoodTester.Infrastructure.MessageBus.Messages;

namespace AnalysisEngine.Consumers
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private const string ExchangeName = "food_analysis";
        private const string QueueName = "analysis_requests";

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger)
        {
            _logger = logger;
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
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

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<FoodAnalysisMessage>(
                    Encoding.UTF8.GetString(body));

                try
                {
                    ProcessMessage(message);
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

        private void ProcessMessage(FoodAnalysisMessage message)
        {
            _logger.LogInformation("Processing analysis request for serial number: {SerialNumber}",
                message.SerialNumber);
            // TODO: implement the logic to create and manage AnalysisWorker containers
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
