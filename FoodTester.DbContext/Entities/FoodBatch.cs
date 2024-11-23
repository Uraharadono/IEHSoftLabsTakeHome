using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FoodTester.Infrastructure.DbUtility;
using System.Collections.Generic;

namespace FoodTester.DbContext.Entities
{
    public class FoodBatch : IEntity, ITimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        [Column(TypeName = "VARCHAR")]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string SerialNumber { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        /* =================== Navigation properties =================== */

        public ICollection<AnalysisRequest> AnalysisRequests { get; set; }
    }
}
