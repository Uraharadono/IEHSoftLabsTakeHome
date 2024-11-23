namespace AnalysisWorker
{
    public class AnalysisWorker : BackgroundService
    {
        private readonly ILogger<AnalysisWorker> _logger;
        private readonly string _serialNumber;
        private readonly string _foodType;
        private readonly string[] _requiredAnalyses;

        public AnalysisWorker(ILogger<AnalysisWorker> logger)
        {
            _logger = logger;
            _serialNumber = Environment.GetEnvironmentVariable("SERIAL_NUMBER") ?? "";
            _foodType = Environment.GetEnvironmentVariable("FOOD_TYPE") ?? "";
            _requiredAnalyses = Environment.GetEnvironmentVariable("REQUIRED_ANALYSES")?.Split(",")
                ?? Array.Empty<string>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting analysis for serial number: {SerialNumber}", _serialNumber);

            foreach (var analysis in _requiredAnalyses)
            {
                _logger.LogInformation("Performing {Analysis} analysis", analysis);

                // Simulate analysis work
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                _logger.LogInformation("Completed {Analysis} analysis", analysis);
            }

            _logger.LogInformation("All analyses completed for serial number: {SerialNumber}", _serialNumber);
        }
    }
}
