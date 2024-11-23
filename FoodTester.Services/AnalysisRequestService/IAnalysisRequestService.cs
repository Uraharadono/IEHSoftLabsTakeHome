using FoodTester.Infrastructure.Services;
using FoodTester.Services.AnalysisRequestService.Dtos;
using System.Threading.Tasks;

namespace FoodTester.Services.AnalysisRequestService
{
    public interface IAnalysisRequestService : IService
    {
        Task<AnalysisRequestDto> CreateAnalysisRequestAsync(AnalysisRequestDto analysisRequestDto);
        Task<AnalysisRequestDto> GetAnalysisRequestAsync(long id);
    }
}
