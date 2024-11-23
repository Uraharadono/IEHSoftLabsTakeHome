using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FoodTester.Infrastructure.DbUtility;

namespace FoodTester.DbContext.Entities
{
    public class AnalysisResult : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(MAX)")]
        public string ResultData { get; set; }

        public DateTime? CompletedAt { get; set; }

        /* =================== Navigation properties =================== */

        [Required]
        public long RequestId { get; set; }

        [ForeignKey("RequestId")]
        public AnalysisRequest AnalysisRequest { get; set; }
    }
}
