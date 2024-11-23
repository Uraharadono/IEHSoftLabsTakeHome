using FoodTester.DbContext.Entities;
using FoodTester.Services.AnalysisRequestService.Dtos;
using System.Threading.Tasks;

namespace FoodTester.Services.AnalysisRequestService
{
    public interface IAnalysisRequestService
    {
        Task<AnalysisRequestDto> CreateAnalysisRequestAsync(AnalysisRequestDto analysisRequestDto);
        Task<AnalysisRequestDto> GetAnalysisRequestAsync(long id);
    }
}
