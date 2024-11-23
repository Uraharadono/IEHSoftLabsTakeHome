using FoodTester.Services.FoodBatchService.Dtos;
using System.ComponentModel.DataAnnotations;

namespace FoodTester.Api.Models
{
    public class FoodBatchViewModel
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string SerialNumber { get; set; }

        public static explicit operator FoodBatchDto(FoodBatchViewModel model)
        {
            return new FoodBatchDto
            {
                Name = model.Name,
                SerialNumber = model.SerialNumber
            };
        }
    }
}
