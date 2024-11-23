using AnalysisEngine.Consumers;
using FoodTester.Infrastructure.Settings;

namespace AnalysisEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.Configure<AppSettings>(options => builder.Configuration.GetSection("AppSettings").Bind(options));
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHostedService<RabbitMQConsumer>();

            var host = builder.Build();
            host.Run();
        }
    }
}