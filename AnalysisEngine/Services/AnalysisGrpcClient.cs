using Grpc.Net.Client;
using FoodTester.Infrastructure.Grpc;

namespace AnalysisEngine.Services
{
    public interface IAnalysisGrpcClient
    {
        Task<AnalysisResponse> StartAnalysisAsync(string serialNumber, string foodType, string[] requiredAnalyses);
        Task<StatusResponse> GetAnalysisStatusAsync(string analysisId);
    }

    public class AnalysisGrpcClient : IAnalysisGrpcClient
    {
        private readonly AnalysisService.AnalysisServiceClient _client;
        private readonly ILogger<AnalysisGrpcClient> _logger;

        public AnalysisGrpcClient(ILogger<AnalysisGrpcClient> logger)
        {
            _logger = logger;
            var channel = GrpcChannel.ForAddress("http://localhost:5000"); // Configure port as needed
            _client = new AnalysisService.AnalysisServiceClient(channel);
        }

        public async Task<AnalysisResponse> StartAnalysisAsync(string serialNumber, string foodType, string[] requiredAnalyses)
        {
            try
            {
                var request = new AnalysisRequest
                {
                    SerialNumber = serialNumber,
                    FoodType = foodType
                };
                request.RequiredAnalyses.AddRange(requiredAnalyses);

                return await _client.StartAnalysisAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting analysis via gRPC");
                throw;
            }
        }

        public async Task<StatusResponse> GetAnalysisStatusAsync(string analysisId)
        {
            try
            {
                var request = new StatusRequest { AnalysisId = analysisId };
                return await _client.GetAnalysisStatusAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analysis status via gRPC");
                throw;
            }
        }
    }
}