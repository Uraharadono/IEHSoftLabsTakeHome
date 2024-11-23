using System.Collections.Generic;

namespace FoodTester.Utility.SharedModels
{
    // Used mainly for (now) for recursive elements - e.g. "article types" tree
    public class TreeDropdownModel
    {
        public string Name { get; set; }
        public long Value { get; set; }
        public bool HasChildren { get; set; }

        public List<TreeDropdownModel> Children { get; set; }
    }
}
