using AnalysisWorker.Services;

namespace AnalysisWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Explicitly configure Kestrel to listen on all interfaces
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(8080); // Ensure it listens on all interfaces
            });

            builder.Services.AddGrpc();
            builder.Logging.AddConsole();

            var host = builder.Build();
            host.MapGrpcService<AnalysisGrpcService>();

            Console.WriteLine("AnalysisWorker gRPC service is starting...");

            host.Run();
        }
    }
}