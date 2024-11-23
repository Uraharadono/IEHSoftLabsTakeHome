using System;

namespace FoodTester.Utility.Attributes
{
    public class EnumData : Attribute
    {
        public EnumData(object data)
        {
            Data = data;
        }

        public object Data { get; set; }
    }
}
