﻿using FoodTester.Infrastructure.Services;
using FoodTester.Services.FoodBatchService.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodTester.Services.FoodBatchService
{
    public interface IFoodBatchService : IService
    {
        Task<FoodBatchDto> CreateFoodBatchAsync(FoodBatchDto foodBatchDto);
        Task<FoodBatchDto> GetFoodBatchAsync(long id, bool includeAnalysisRequests = false);
        Task<bool> UpdateFoodBatchAnalysis(FoodBatchAnalysisResultsDto dto);
        Task<List<string>> GetAnalysisResults(string serialNumber);
    }
}
