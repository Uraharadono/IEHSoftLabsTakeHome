using FoodTester.DbContext.Infrastructure;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FoodTester.DbContext.Seeders.Base
{
    public class DatabaseSeeder
    {
        public FoodQualityContext DbContext { get; }

        public DatabaseSeeder(FoodQualityContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task Seed(bool debug = false)
        {
            if (!debug) return;

            // DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

            try
            {
                await new AnalysisTypesSeeder(DbContext).Seed();
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();

                while (ex != null)
                {
                    sb.AppendLine(ex.Message);
                    sb.AppendLine(ex.StackTrace);

                    ex = ex.InnerException;
                }

                throw new Exception($"Error seeding data - errors follow:{Environment.NewLine}{sb}", ex);
            }
        }
    }
}
