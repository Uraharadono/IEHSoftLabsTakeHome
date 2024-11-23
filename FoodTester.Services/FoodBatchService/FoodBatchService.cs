using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodTester.DbContext.Entities;
using FoodTester.DbContext.Infrastructure;
using FoodTester.Services.FoodBatchService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FoodTester.Services.FoodBatchService
{
    public class FoodBatchService : IFoodBatchService
    {
        private readonly FoodQualityContext _context;

        public FoodBatchService(FoodQualityContext context)
        {
            _context = context;
        }

        public async Task<FoodBatchDto> CreateFoodBatchAsync(FoodBatchDto foodBatchDto)
        {
            var foodBatch = (FoodBatch)foodBatchDto;
            await _context.FoodBatches.AddAsync(foodBatch);
            await _context.SaveChangesAsync();
            return (FoodBatchDto)foodBatch;
        }

        public async Task<FoodBatchDto> GetFoodBatchAsync(long id)
        {
            return (FoodBatchDto)await _context.FoodBatches.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<string>> GetAnalysisResults(string serialNumber)
        {
            var foodBatch = await _context.FoodBatches
                                          .Include(f => f.AnalysisRequests)
                                          .ThenInclude(a => a.AnalysisResult)
                                          .FirstOrDefaultAsync(f => f.SerialNumber == serialNumber);
            if (foodBatch == null)
            {
                return null;
            }

            return foodBatch.AnalysisRequests.Select(a => a.AnalysisResult.ResultData).ToList();
        }
    }
}
