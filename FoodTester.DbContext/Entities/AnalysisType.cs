using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FoodTester.Infrastructure.DbUtility;

namespace FoodTester.DbContext.Entities
{
    public class AnalysisType : IEntity, ITimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string TypeName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
    }
}
