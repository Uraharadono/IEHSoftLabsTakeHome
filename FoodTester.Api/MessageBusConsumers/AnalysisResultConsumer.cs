using FoodTester.Infrastructure.MessageBus.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using FoodTester.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using FoodTester.Services.FoodBatchService;
using FoodTester.Services.FoodBatchService.Dtos;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace FoodTester.Api.MessageBusConsumers
{
    public class AnalysisResultConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<AnalysisResultConsumer> _logger;
        private const string ExchangeName = "food_analysis";
        private const string QueueName = "analysis_results_queue";
        private const string RoutingKey = "analysis_results";

        public AnalysisResultConsumer(IOptions<AppSettings> settings, ILogger<AnalysisResultConsumer> logger, IServiceProvider serviceProvider)
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

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var message = JsonSerializer.Deserialize<AnalysisResultMessage>(Encoding.UTF8.GetString(body));

                    // Process the result.
                    await ProcessAnalysisResults(message);

                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing analysis results");
                    _channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessAnalysisResults(AnalysisResultMessage message)
        {
            FoodBatchAnalysisResultsDto dto = new FoodBatchAnalysisResultsDto();
            dto.SerialNumber = message.SerialNumber;
            dto.AnalysisResults = message.Results.Select(s => new Services.AnalysisRequestService.Dtos.AnalysisResultDto
            {
                AnalysisId = s.AnalysisId,
                ResultData = s.Details
            }).ToList();

            using var scope = _serviceProvider.CreateScope();
            var _foodBatchService = scope.ServiceProvider.GetRequiredService<IFoodBatchService>();
            await _foodBatchService.UpdateFoodBatchAnalysis(dto);
            _logger.LogInformation("Received analysis results for serial number: {SerialNumber}", message.SerialNumber);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
