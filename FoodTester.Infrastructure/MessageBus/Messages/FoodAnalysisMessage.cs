using System;

namespace FoodTester.Infrastructure.MessageBus.Messages
{
    public record FoodAnalysisMessage
    {
        public string SerialNumber { get; init; }
        public string FoodType { get; init; }
        public FoodAnalysisType[] RequiredAnalyses { get; init; }
        public DateTime RequestedAt { get; init; }
    }

    public record FoodAnalysisType
    {
        public long AnalysisId { get; set; }
        public string AnalysisName { get; set; }
    }
}
