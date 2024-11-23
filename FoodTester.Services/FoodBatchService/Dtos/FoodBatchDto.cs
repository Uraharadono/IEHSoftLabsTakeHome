using FoodTester.DbContext.Entities;

namespace FoodTester.Services.FoodBatchService.Dtos
{
    public class FoodBatchDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string SerialNumber { get; set; }

        public static explicit operator FoodBatch(FoodBatchDto model)
        {
            return new FoodBatch
            {
                Id = model.Id,
                Name = model.Name,
                SerialNumber = model.SerialNumber
            };
        }

        public static explicit operator FoodBatchDto(FoodBatch model)
        {
            return new FoodBatchDto
            {
                Id = model.Id,
                Name = model.Name,
                SerialNumber = model.SerialNumber
            };
        }
    }
}
