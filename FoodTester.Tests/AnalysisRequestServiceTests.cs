using FoodTester.DbContext.Entities;
using FoodTester.DbContext.Enums;
using FoodTester.DbContext.Infrastructure;
using FoodTester.Services.AnalysisRequestService;
using FoodTester.Services.AnalysisRequestService.Dtos;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

namespace FoodTester.Tests
{
    public class AnalysisRequestServiceTests
    {
        private FoodQualityContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FoodQualityContext>()
                .UseInMemoryDatabase(databaseName: "FoodQualityTestDb")
                .Options;
            return new FoodQualityContext(options);
        }

        [Fact]
        public async Task CreateAnalysisRequestAsync_ValidDto_ReturnsCreatedAnalysisRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AnalysisRequestService(context);
            var analysisRequestDto = new AnalysisRequestDto
            {
                BatchId = 1,
                AnalysisTypeId = 1
            };

            // Act
            var result = await service.CreateAnalysisRequestAsync(analysisRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(analysisRequestDto.BatchId, result.BatchId);
            Assert.Equal(analysisRequestDto.AnalysisTypeId, result.AnalysisTypeId);
        }

        [Fact]
        public async Task GetAnalysisRequestAsync_ExistingId_ReturnsAnalysisRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AnalysisRequestService(context);
            var analysisRequest = new AnalysisRequest
            {
                Id = 1,
                BatchId = 1,
                AnalysisTypeId = 1,
                Status = EAnalysisRequestStatus.IN_PROGRESS.ToString()
            };
            context.AnalysisRequests.Add(analysisRequest);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAnalysisRequestAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.BatchId);
            Assert.Equal(1, result.AnalysisTypeId);
            Assert.Equal(EAnalysisRequestStatus.IN_PROGRESS.ToString(), result.Status);
        }

        [Fact]
        public async Task GetAnalysisRequestAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AnalysisRequestService(context);

            // Act
            var result = await service.GetAnalysisRequestAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
