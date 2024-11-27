using Microsoft.EntityFrameworkCore;
using FoodTester.DbContext.Entities;
using System.Diagnostics;

namespace FoodTester.DbContext.Infrastructure
{
    public class FoodQualityContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public FoodQualityContext(DbContextOptions<FoodQualityContext> options) : base(options)
        {
        }

        // This method won't be used in the Api project anymore, but it is still used in CronJobs (Background Tasks)
        public static FoodQualityContext Create(string connection)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FoodQualityContext>();
            optionsBuilder.UseSqlServer(connection);

            // Helps me with debugging stuff
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(message => Debug.WriteLine(message)); // https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/simple-logging

            return new FoodQualityContext(optionsBuilder.Options);
        }

        public DbSet<FoodBatch> FoodBatches { get; set; }
        public DbSet<AnalysisType> AnalysisTypes { get; set; }
        public DbSet<AnalysisRequest> AnalysisRequests { get; set; }
        public DbSet<AnalysisResult> AnalysisResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=.;Database=FoodTesterDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=true;");

                // Helps me with debugging stuff
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(message => Debug.WriteLine(message)); // https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/simple-logging
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
        }
    }
}
