using FoodTester.Infrastructure.MessageBus.Messages;
using System.Threading.Tasks;

namespace FoodTester.Services.MessageBus.Publishers
{
    public interface IRabbitMQPublisher
    {
        Task PublishAnalysisRequestAsync(FoodAnalysisMessage message);
    }
}
