using FoodTester.DbContext.Infrastructure;
using FoodTester.Utility.Data;
using System;
using System.Threading.Tasks;

namespace FoodTester.DbContext.Seeders.Base
{
    public abstract class BaseSeeder
    {
        protected int NumberOfElementsToSeed { get; set; }
        protected readonly Random Rand;
        protected readonly LoremIpsumGenerator LoremIpsum;

        protected FoodQualityContext DbContext { get; }

        protected BaseSeeder(FoodQualityContext dbContext)
        {
            DbContext = dbContext;

            NumberOfElementsToSeed = 50;
            Rand = new Random(DateTime.Now.Millisecond);
            LoremIpsum = new LoremIpsumGenerator(Rand);
        }

        public abstract Task Seed();

        // Logs to file {solution folder}\seed.log data from Seed method (for DEBUG only)
        protected void Log(string msg)
        {
            string echoCmd = $"/C echo {DateTime.Now} - {msg} >> seed.log";
            System.Diagnostics.Process.Start("cmd.exe", echoCmd)?.WaitForExit();
        }
    }
}
