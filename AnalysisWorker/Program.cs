using AnalysisWorker.Services;
using Microsoft.AspNetCore.Builder;

namespace AnalysisWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();
            builder.Services.AddHostedService<AnalysisWorker>();

            var host = builder.Build();

            host.MapGrpcService<AnalysisGrpcService>();

            host.Run();
        }
    }
}