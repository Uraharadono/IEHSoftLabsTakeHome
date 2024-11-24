using Grpc.Core;
using FoodTester.Infrastructure.Grpc;
using System.Text.Json;

namespace AnalysisWorker.Services
{
    public class AnalysisGrpcService : AnalysisService.AnalysisServiceBase
    {
        private readonly ILogger<AnalysisGrpcService> _logger;
        private readonly Dictionary<string, AnalysisStatus> _analysisStatuses;
        private readonly Dictionary<string, List<AnalysisResult>> _analysisResults;

        public AnalysisGrpcService(ILogger<AnalysisGrpcService> logger)
        {
            _logger = logger;
            _analysisStatuses = new Dictionary<string, AnalysisStatus>();
            _analysisResults = new Dictionary<string, List<AnalysisResult>>();
        }

        public override async Task<AnalysisResponse> StartAnalysis(AnalysisRequest request, ServerCallContext context)
        {
            var analysisId = Guid.NewGuid().ToString();
            _logger.LogInformation("AnalysisGrpService.cs -> Starting analysis {AnalysisId} for serial number {SerialNumber}",
                analysisId, request.SerialNumber);
            _logger.LogInformation($"Object: {JsonSerializer.Serialize(request)}");

            try
            {
                _analysisStatuses[analysisId] = AnalysisStatus.InProgress;
                _analysisResults[analysisId] = new List<AnalysisResult>();

                // Start analysis in background
                _ = ProcessAnalysisAsync(analysisId, request);

                return new AnalysisResponse
                {
                    AnalysisId = analysisId,
                    Success = true,
                    Message = "Analysis started successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start analysis for {SerialNumber}", request.SerialNumber);
                return new AnalysisResponse
                {
                    AnalysisId = analysisId,
                    Success = false,
                    Message = $"Failed to start analysis: {ex.Message}"
                };
            }
        }

        public override Task<StatusResponse> GetAnalysisStatus(StatusRequest request, ServerCallContext context)
        {
            if (!_analysisStatuses.ContainsKey(request.AnalysisId))
            {
                return Task.FromResult(new StatusResponse
                {
                    AnalysisId = request.AnalysisId,
                    Status = AnalysisStatus.Unknown,
                    ErrorMessage = "Analysis ID not found"
                });
            }

            var response = new StatusResponse
            {
                AnalysisId = request.AnalysisId,
                Status = _analysisStatuses[request.AnalysisId]
            };

            if (_analysisResults.ContainsKey(request.AnalysisId))
            {
                response.Results.AddRange(_analysisResults[request.AnalysisId]);
            }

            return Task.FromResult(response);
        }

        private async Task ProcessAnalysisAsync(string analysisId, AnalysisRequest request)
        {
            try
            {
                foreach (var analysisType in request.RequiredAnalyses)
                {
                    // Simulate analysis work
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    var result = new AnalysisResult
                    {
                        AnalysisType = analysisType,
                        Passed = Random.Shared.Next(100) > 20, // 80% pass rate for simulation
                        Details = $"Completed {analysisType} analysis",
                        Value = Random.Shared.NextDouble() * 100,
                        Unit = "units"
                    };

                    _analysisResults[analysisId].Add(result);
                }

                _analysisStatuses[analysisId] = AnalysisStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis {AnalysisId} failed", analysisId);
                _analysisStatuses[analysisId] = AnalysisStatus.Failed;
            }
        }
    }
}