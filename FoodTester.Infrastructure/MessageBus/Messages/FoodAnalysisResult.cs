using System;
using System.Collections.Generic;

namespace FoodTester.Infrastructure.MessageBus.Messages
{
    public record AnalysisResultMessage
    {
        public string SerialNumber { get; init; }
        public DateTime CompletedAt { get; init; }
        public List<AnalysisResultDetail> Results { get; init; }
    }

    public record AnalysisResultDetail
    {
        public long AnalysisId { get; init; }
        public string AnalysisType { get; init; }
        public bool Passed { get; init; }
        public double Value { get; init; }
        public string Unit { get; init; }
        public string Details { get; init; }
    }
}
