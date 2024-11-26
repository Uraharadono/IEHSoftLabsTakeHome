using FoodTester.Api.Controllers;
using FoodTester.Api.Models;
using FoodTester.Services.AnalysisRequestService;
using FoodTester.Services.AnalysisRequestService.Dtos;
using FoodTester.Services.FoodBatchService;
using FoodTester.Services.FoodBatchService.Dtos;
using FoodTester.Services.MessageBus.Publishers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace FoodTester.Tests.Controllers
{
    public class QualityManagerControllerTests
    {
        private readonly QualityManagerController _controller;
        private readonly Mock<IFoodBatchService> _foodBatchServiceMock;
        private readonly Mock<ILogger<QualityManagerController>> _loggerMock;

        public QualityManagerControllerTests()
        {
            _foodBatchServiceMock = new Mock<IFoodBatchService>();
            _loggerMock = new Mock<ILogger<QualityManagerController>>();
            _controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, null, null);
        }


        [Fact]
        public async Task CreateFoodBatch_ValidModel_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var model = new FoodBatchViewModel { /* set properties */ };
            var foodBatchDto = new FoodBatchDto { Id = 1, SerialNumber = "12345" };
            _foodBatchServiceMock.Setup(service => service.CreateFoodBatchAsync(It.IsAny<FoodBatchDto>()))
                                 .ReturnsAsync(foodBatchDto);

            // Act
            var result = await _controller.CreateFoodBatch(model);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal(foodBatchDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateFoodBatch_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.CreateFoodBatch(new FoodBatchViewModel());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAnalysisRequest_ValidModel_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var model = new AnalysisRequestViewModel { /* set properties */ };
            var analysisRequestDto = new AnalysisRequestDto { Id = 1, BatchId = 1 };
            var analysisRequestServiceMock = new Mock<IAnalysisRequestService>();
            analysisRequestServiceMock.Setup(service => service.CreateAnalysisRequestAsync(It.IsAny<AnalysisRequestDto>()))
                                      .ReturnsAsync(analysisRequestDto);
            var controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, analysisRequestServiceMock.Object, null);

            // Act
            var result = await controller.CreateAnalysisRequest(model);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal(analysisRequestDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateAnalysisRequest_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var analysisRequestServiceMock = new Mock<IAnalysisRequestService>();
            var controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, analysisRequestServiceMock.Object, null);
            controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await controller.CreateAnalysisRequest(new AnalysisRequestViewModel());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task StartBatchProcess_ExistingId_ReturnsOk()
        {
            // Arrange
            var foodBatchDto = new FoodBatchDto { Id = 1, SerialNumber = "12345", AnalysisRequests = new List<AnalysisRequestDto>() };
            _foodBatchServiceMock.Setup(service => service.GetFoodBatchAsync(It.IsAny<long>(), true))
                                 .ReturnsAsync(foodBatchDto);
            var publisherMock = new Mock<IRabbitMQPublisher>();
            var controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, null, publisherMock.Object);

            // Act
            var result = await controller.StartBatchProcess(1);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task StartBatchProcess_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _foodBatchServiceMock.Setup(service => service.GetFoodBatchAsync(It.IsAny<long>(), true))
                                 .ReturnsAsync((FoodBatchDto)null);
            var controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, null, null);

            // Act
            var result = await controller.StartBatchProcess(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetStatus_ExistingSerialNumber_ReturnsOk()
        {
            // Arrange
            var analysisResults = new List<string> { "Result1", "Result2" };
            _foodBatchServiceMock.Setup(service => service.GetAnalysisResults(It.IsAny<string>()))
                                 .ReturnsAsync(analysisResults);
            var controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, null, null);

            // Act
            var result = await controller.GetStatus("12345");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(analysisResults, okResult.Value);
        }

        [Fact]
        public async Task GetStatus_NonExistingSerialNumber_ReturnsNotFound()
        {
            // Arrange
            _foodBatchServiceMock.Setup(service => service.GetAnalysisResults(It.IsAny<string>()))
                                 .ReturnsAsync((List<string>)null);
            var controller = new QualityManagerController(_loggerMock.Object, _foodBatchServiceMock.Object, null, null);

            // Act
            var result = await controller.GetStatus("12345");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}