using FoodTester.Infrastructure.MessageBus.Messages;

namespace AnalysisEngine.Publishers
{
    public interface IResultPublisher
    {
        Task PublishAnalysisResultsAsync(AnalysisResultMessage resultMessage);
    }
}
