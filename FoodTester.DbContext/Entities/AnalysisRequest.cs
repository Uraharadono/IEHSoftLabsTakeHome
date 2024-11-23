using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FoodTester.Infrastructure.DbUtility;
using FoodTester.DbContext.Enums;

namespace FoodTester.DbContext.Entities
{
    public class AnalysisRequest : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(50)")]
        [EnumDataType(typeof(EAnalysisRequestStatus))]
        public string Status { get; set; }

        public DateTime? CompletedAt { get; set; }

        /* =================== Navigation properties =================== */
        public AnalysisResult AnalysisResult { get; set; }

        [Required]
        public long AnalysisTypeId { get; set; }

        [ForeignKey("AnalysisTypeId")]
        public AnalysisType AnalysisType { get; set; }

        [Required]
        public long BatchId { get; set; }

        [ForeignKey("BatchId")]
        public FoodBatch FoodBatch { get; set; }
    }
}
