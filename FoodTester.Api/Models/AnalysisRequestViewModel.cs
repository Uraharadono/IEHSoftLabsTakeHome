using FoodTester.DbContext.Enums;
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

        [Required]
        [EnumDataType(typeof(EAnalysisRequestStatus))]
        public string Status { get; set; }


        public static explicit operator AnalysisRequestDto(AnalysisRequestViewModel model)
        {
            return new AnalysisRequestDto
            {
                BatchId = model.BatchId,
                AnalysisTypeId = model.AnalysisTypeId,
                Status = model.Status
            };
        }
    }
}
