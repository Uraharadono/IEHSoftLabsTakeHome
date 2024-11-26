using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FoodTester.Infrastructure.DbUtility;

namespace FoodTester.DbContext.Entities
{
    public class AnalysisResult : IEntity
    {
        [ForeignKey("AnalysisRequest")]
        public long Id { get; set; }
        public virtual AnalysisRequest AnalysisRequest { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(MAX)")]
        public string ResultData { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
