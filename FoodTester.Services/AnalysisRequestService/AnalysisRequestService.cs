using FoodTester.DbContext.Entities;
using FoodTester.DbContext.Infrastructure;
using FoodTester.Services.AnalysisRequestService.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FoodTester.Services.AnalysisRequestService
{
    public class AnalysisRequestService : IAnalysisRequestService
    {
        private readonly FoodQualityContext _context;

        public AnalysisRequestService(FoodQualityContext context)
        {
            _context = context;
        }

        public async Task<AnalysisRequestDto> CreateAnalysisRequestAsync(AnalysisRequestDto analysisRequestDto)
        {
            var analysisRequest = (AnalysisRequest)analysisRequestDto;
            var dbEntity = await _context.AnalysisRequests.AddAsync(analysisRequest);
            await _context.SaveChangesAsync();
            // await dbEntity.Reference(s => s.FoodBatch).LoadAsync();

            return (AnalysisRequestDto)analysisRequest;
        }

        public async Task<AnalysisRequestDto> GetAnalysisRequestAsync(long id)
        {
            return (AnalysisRequestDto)(await _context.AnalysisRequests.FirstOrDefaultAsync(x => x.Id == id));
        }
    }
}
