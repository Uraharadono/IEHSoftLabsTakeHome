using AnalysisWorker.Services;

namespace AnalysisWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();

            var host = builder.Build();

            host.MapGrpcService<AnalysisGrpcService>();

            host.Run();
        }
    }
}