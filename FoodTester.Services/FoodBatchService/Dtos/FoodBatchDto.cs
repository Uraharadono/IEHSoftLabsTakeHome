using FoodTester.DbContext.Entities;
using FoodTester.Services.AnalysisRequestService.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace FoodTester.Services.FoodBatchService.Dtos
{
    public class FoodBatchDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string SerialNumber { get; set; }

        public List<AnalysisRequestDto> AnalysisRequests { get; set; }

        public static explicit operator FoodBatch(FoodBatchDto model)
        {
            return new FoodBatch
            {
                Id = model.Id,
                Name = model.Name,
                SerialNumber = model.SerialNumber
            };
        }

        public static explicit operator FoodBatchDto(FoodBatch model)
        {
            return model != null ?
             new FoodBatchDto
             {
                 Id = model.Id,
                 Name = model.Name,
                 SerialNumber = model.SerialNumber,
                 AnalysisRequests = model.AnalysisRequests?.Select(ar => (AnalysisRequestDto)ar).ToList()
             } : null;
        }
    }
}
