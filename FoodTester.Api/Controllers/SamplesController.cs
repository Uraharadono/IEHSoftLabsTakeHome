using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FoodTester.Api.Models;
using FoodTester.Api.Utility.Extensions;
using FoodTester.Services.Sample;
using FoodTester.Services.Sample.Dto;
using FoodTester.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace FoodTester.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SamplesController : ControllerBase
    {
        private readonly ISampleService _sampleService;
        private readonly AppSettings _settings;

        public SamplesController(ISampleService SampleService, IOptions<AppSettings> options)
        {
            _sampleService = SampleService;
            _settings = options.Value;
        }

        [HttpGet, Route("get-samples")]
        public async Task<IActionResult> GetSamples()
        {
            var blogs = await _sampleService.GetSamples();
            return blogs != null ? Ok(blogs) : StatusCode(500);
        }

        [HttpGet, Route("get-sample")]
        public async Task<IActionResult> GetSample(long sampleId)
        {
            var blog = await _sampleService.GetSample(sampleId);
            return blog != null ? Ok(blog) : StatusCode(500);
        }

        [HttpPost, Route("add-sample")]
        public async Task<IActionResult> AddSample(SampleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelStateExtensions.GetErrorMessage(ModelState));

            SampleDto dto = new SampleDto
            {
                Id = model.Id,
                Title = model.Title,
                Content = model.Content,
                BlogId = model.BlogId
            };

            var addStatus = await _sampleService.AddSample(dto);
            return addStatus != null ? Ok() : StatusCode(500);
        }

        [HttpPut, Route("update-sample")]
        public async Task<IActionResult> UpdateSample(SampleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelStateExtensions.GetErrorMessage(ModelState));

            SampleDto dto = new SampleDto
            {
                Id = model.Id,
                Title = model.Title,
                Content = model.Content,
                BlogId = model.BlogId
            };

            var addStatus = await _sampleService.UpdateSample(dto);
            return addStatus ? Ok() : StatusCode(500);
        }
    }
}
