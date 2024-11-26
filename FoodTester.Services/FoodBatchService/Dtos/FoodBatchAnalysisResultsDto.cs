using FoodTester.Services.AnalysisRequestService.Dtos;
using System.Collections.Generic;

namespace FoodTester.Services.FoodBatchService.Dtos
{
    public class FoodBatchAnalysisResultsDto
    {
        public string SerialNumber { get; set; }
        public List<AnalysisResultDto> AnalysisResults { get; set; }
    }
}
