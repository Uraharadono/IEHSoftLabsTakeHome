using Grpc.Core;
using AnalysisWorker.Grpc;

namespace AnalysisWorker.Services
{
    public class AnalysisGrpcService : AnalysisService.AnalysisServiceBase
    {
        private readonly ILogger<AnalysisGrpcService> _logger;

        public AnalysisGrpcService(ILogger<AnalysisGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task AnalyzeFood(
            AnalysisRequest request,
            IServerStreamWriter<AnalysisUpdate> responseStream,
            ServerCallContext context)
        {
            _logger.LogInformation($"Starting analysis for serial number {request.SerialNumber}");

            try
            {
                // Send initial status
                await responseStream.WriteAsync(new AnalysisUpdate
                {
                    SerialNumber = request.SerialNumber,
                    Status = AnalysisStatus.InProgress
                });

                // Process each analysis
                foreach (var analysisType in request.RequiredAnalyses)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Simulate analysis work
                    await Task.Delay(TimeSpan.FromSeconds(3), context.CancellationToken);

                    var result = new AnalysisResult
                    {
                        AnalysisId = analysisType.AnalysisId,
                        AnalysisType = analysisType.AnalysisName,
                        Passed = Random.Shared.Next(100) > 20, // 80% pass rate
                        Details = $"Completed {analysisType} analysis",
                        Value = Random.Shared.NextDouble() * 100,
                        Unit = "units"
                    };

                    // Stream the result back
                    await responseStream.WriteAsync(new AnalysisUpdate
                    {
                        SerialNumber = request.SerialNumber,
                        Status = AnalysisStatus.InProgress,
                        Result = result
                    });
                }

                // Send completion status
                await responseStream.WriteAsync(new AnalysisUpdate
                {
                    SerialNumber = request.SerialNumber,
                    Status = AnalysisStatus.Completed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed for {SerialNumber}", request.SerialNumber);

                await responseStream.WriteAsync(new AnalysisUpdate
                {
                    SerialNumber = request.SerialNumber,
                    Status = AnalysisStatus.Failed,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}