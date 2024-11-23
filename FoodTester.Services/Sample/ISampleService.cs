using FoodTester.Infrastructure.Services;
using FoodTester.Services.Sample.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodTester.Services.Sample
{
    public interface ISampleService : IService
    {
        Task<List<SampleDto>> GetSamples();
        Task<SampleDto> GetSample(long postId);
        Task<SampleDto> AddSample(SampleDto dto);
        Task<bool> UpdateSample(SampleDto dto);
    }
}
