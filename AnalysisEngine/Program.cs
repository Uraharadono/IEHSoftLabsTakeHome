using AnalysisEngine.Consumers;
using AnalysisEngine.DockerService;
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
            builder.Services.AddSingleton<IDockerService, AnalysisEngine.DockerService.DockerService>();

            var host = builder.Build();
            host.Run();
        }
    }
}