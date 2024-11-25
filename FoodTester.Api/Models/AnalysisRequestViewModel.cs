using FoodTester.Services.AnalysisRequestService.Dtos;
using System.ComponentModel.DataAnnotations;

namespace FoodTester.Api.Models
{
    public class AnalysisRequestViewModel
    {
        [Required]
        public long BatchId { get; set; }

        [Required]
        public long AnalysisTypeId { get; set; }


        public static explicit operator AnalysisRequestDto(AnalysisRequestViewModel model)
        {
            return new AnalysisRequestDto
            {
                BatchId = model.BatchId,
                AnalysisTypeId = model.AnalysisTypeId,
            };
        }
    }
}
