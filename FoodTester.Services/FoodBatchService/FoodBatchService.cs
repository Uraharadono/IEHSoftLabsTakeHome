using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodTester.DbContext.Entities;
using FoodTester.DbContext.Enums;
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

        public async Task<FoodBatchDto> GetFoodBatchAsync(long id, bool includeAnalysisRequests = false)
        {
            IQueryable<FoodBatch> query = _context.FoodBatches;
            if (includeAnalysisRequests)
                query = query.Include(ar => ar.AnalysisRequests).ThenInclude(at => at.AnalysisType);

            return (FoodBatchDto)await query.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> UpdateFoodBatchAnalysis(FoodBatchAnalysisResultsDto dto)
        {
            try
            {
                if (dto == null) return false;
                var fBatch = await _context.FoodBatches.FirstOrDefaultAsync(s => s.SerialNumber == dto.SerialNumber);
                fBatch.ModifiedAt = DateTime.UtcNow;

                var analysisRequests = await _context.AnalysisRequests.Where(s => dto.AnalysisResults.Select(d => d.AnalysisId).Contains(s.Id)).ToListAsync();
                foreach (var analysisRequest in analysisRequests)
                {
                    analysisRequest.Status = EAnalysisRequestStatus.COMPLETED.ToString();
                }

                var resultsList = new List<AnalysisResult>();
                foreach (var analysisResult in dto.AnalysisResults)
                {
                    resultsList.Add(new AnalysisResult
                    {
                        Id = analysisResult.AnalysisId,
                        ResultData = analysisResult.ResultData,
                        CompletedAt = DateTime.UtcNow
                    });
                }

                await _context.AnalysisResults.AddRangeAsync(resultsList);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {

                throw;
            }
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

            if (foodBatch == null || !foodBatch.AnalysisRequests.Any() || foodBatch.AnalysisRequests.All(s => s.AnalysisResult == null))
                return null;
            return foodBatch.AnalysisRequests.Select(a => a.AnalysisResult?.ResultData).ToList();
        }
    }
}
