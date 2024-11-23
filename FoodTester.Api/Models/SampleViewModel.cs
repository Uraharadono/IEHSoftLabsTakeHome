using System.ComponentModel.DataAnnotations;

namespace FoodTester.Api.Models
{
    public class SampleViewModel
    {
        public long Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public long BlogId { get; set; }
    }
}
