using FoodTester.Infrastructure.MessageBus.Messages;

namespace AnalysisEngine.DockerService
{
    public interface IDockerService
    {
        Task<string> StartAnalysisWorkerAsync(FoodAnalysisMessage message);
        Task StopContainerAsync(string containerId);
        Task<string> GetContainerLogsAsync(string containerId);
        Task<ContainerInfo> GetContainerInfoAsync(string containerId);
    }
}
