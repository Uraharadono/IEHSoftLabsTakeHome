using System;

namespace FoodTester.Infrastructure.MessageBus.Messages
{
    public record FoodAnalysisMessage
    {
        public string SerialNumber { get; init; }
        public string FoodType { get; init; }
        public string[] RequiredAnalyses { get; init; }
        public DateTime RequestedAt { get; init; }
    }
}
