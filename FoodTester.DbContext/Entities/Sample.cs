using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodTester.Infrastructure.DbUtility;

namespace FoodTester.DbContext.Entities
{
    public class Sample : IEntity, ITimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public long BlogId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
    }
}
