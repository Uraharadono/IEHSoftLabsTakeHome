using FoodTester.DbContext.Entities;
using FoodTester.DbContext.Enums;

namespace FoodTester.Services.AnalysisRequestService.Dtos
{
    public class AnalysisRequestDto
    {
        public long Id { get; set; }
        public long BatchId { get; set; }

        public long AnalysisTypeId { get; set; }
        public string AnalysisType { get; set; }

        public string Status { get; set; }


        public static explicit operator AnalysisRequest(AnalysisRequestDto model)
        {
            return new AnalysisRequest
            {
                Id = model.Id,
                BatchId = model.BatchId,
                AnalysisTypeId = model.AnalysisTypeId,
                Status = model.Status ?? EAnalysisRequestStatus.IN_PROGRESS.ToString()
            };
        }

        public static explicit operator AnalysisRequestDto(AnalysisRequest model)
        {
            return new AnalysisRequestDto
            {
                Id = model.Id,
                BatchId = model.BatchId,
                AnalysisTypeId = model.AnalysisTypeId,
                AnalysisType = model.AnalysisType?.TypeName,
                Status = model.Status
            };
        }
    }
}
