using FoodTester.DbContext.Entities;
using FoodTester.DbContext.Infrastructure;
using FoodTester.DbContext.Seeders.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodTester.DbContext.Seeders
{
    public class AnalysisTypesSeeder : BaseSeeder
    {
        public AnalysisTypesSeeder(FoodQualityContext dbContext) : base(dbContext)
        {
        }

        public override async Task Seed()
        {
            var analysisTypes = new List<AnalysisType>();

            for (var i = 0; i < NumberOfElementsToSeed * 5; i++)
            {
                analysisTypes.Add(new AnalysisType
                {
                    CreatedAt = DateTime.Now,
                    TypeName = LoremIpsum.GetWord()
                });
            }

            DbContext.AnalysisTypes.AddRange(analysisTypes);
            DbContext.SaveChanges();
        }
    }
}
