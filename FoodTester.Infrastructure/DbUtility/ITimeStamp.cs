using System;

namespace FoodTester.Infrastructure.DbUtility
{
    public interface ITimeStamp
    {
        DateTime CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
    }
}
