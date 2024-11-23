using FoodTester.Utility.Attributes;

namespace FoodTester.DbContext.Enums
{
    public enum EAnalysisRequestStatus
    {
        [EnumDescription("In progress", 0)]
        IN_PROGRESS = 0,

        [EnumDescription("Completed", 1)]
        COMPLETED = 1,

        [EnumDescription("Failed", 2)]
        FAILED = 2
    }
}
