using FoodTester.Api.Models;
using FoodTester.DbContext.Entities;
using FoodTester.Services.AnalysisRequestService;
using FoodTester.Services.AnalysisRequestService.Dtos;
using FoodTester.Services.FoodBatchService;
using FoodTester.Services.FoodBatchService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FoodTester.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QualityManagerController : ControllerBase
    {
        private readonly ILogger<QualityManagerController> _logger;
        private readonly IFoodBatchService _foodBatchService;
        private readonly IAnalysisRequestService _analysisRequestService;

        public QualityManagerController(
            ILogger<QualityManagerController> logger,
            IFoodBatchService foodBatchService,
            IAnalysisRequestService analysisRequestService)
        {
            _logger = logger;
            _foodBatchService = foodBatchService;
            _analysisRequestService = analysisRequestService;
        }

        [HttpPost("food-batch")]
        [ProducesResponseType(typeof(FoodBatchDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateFoodBatch([FromBody] FoodBatchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var foodBatchDto = (FoodBatchDto)model;
            var createdFoodBatch = await _foodBatchService.CreateFoodBatchAsync(foodBatchDto);

            _logger.LogInformation("Created new food batch with SerialNumber: {SerialNumber}", createdFoodBatch.SerialNumber);

            return CreatedAtAction(nameof(GetFoodBatch), new { id = createdFoodBatch.Id }, createdFoodBatch);
        }

        [HttpGet("food-batch/{id}")]
        [ProducesResponseType(typeof(FoodBatchDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFoodBatch(long id)
        {
            var foodBatch = await _foodBatchService.GetFoodBatchAsync(id);

            if (foodBatch == null)
            {
                return NotFound();
            }

            return Ok(foodBatch);
        }

        [HttpPost("analysis-request")]
        [ProducesResponseType(typeof(AnalysisRequestDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateAnalysisRequest([FromBody] AnalysisRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var analysisRequestDto = (AnalysisRequestDto)model;
            var createdAnalysisRequest = await _analysisRequestService.CreateAnalysisRequestAsync(analysisRequestDto);

            _logger.LogInformation("Created new analysis request for BatchId: {BatchId}", createdAnalysisRequest.BatchId);

            return CreatedAtAction(nameof(GetAnalysisRequest), new { id = createdAnalysisRequest.Id }, createdAnalysisRequest);
        }

        [HttpGet("analysis-request/{id}")]
        [ProducesResponseType(typeof(AnalysisRequestDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAnalysisRequest(long id)
        {
            var analysisRequest = await _analysisRequestService.GetAnalysisRequestAsync(id);

            if (analysisRequest == null)
            {
                return NotFound();
            }

            return Ok(analysisRequest);
        }

        [HttpGet("status/{serialNumber}")]
        [ProducesResponseType(typeof(AnalysisResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStatus(string serialNumber)
        {
            var analysisResults = _foodBatchService.GetAnalysisResults(serialNumber);

            if (analysisResults == null)
            {
                return NotFound();
            }

            return Ok(analysisResults);
        }
    }
}
