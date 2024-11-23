using FoodTester.DbContext.Infrastructure;
using FoodTester.Services.Sample.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTester.Services.Sample
{
    public class SampleService : ISampleService
    {
        private readonly FoodQualityContext _dbContext;

        public SampleService(FoodQualityContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SampleDto>> GetSamples()
        {
            var dbSamples = await _dbContext.Samples.ToListAsync();

            if (!dbSamples.Any())
                return null;

            return dbSamples.Select(s => new SampleDto
            {
                Id = s.Id,
                BlogId = s.BlogId,
                Content = s.Content,
                Title = s.Title
            }).ToList();
        }

        public async Task<SampleDto> GetSample(long sampleId)
        {
            var dbSample = await _dbContext.Samples.FirstOrDefaultAsync(s => s.Id == sampleId);
            if (dbSample == null)
                return null;

            return new SampleDto
            {
                Id = dbSample.Id,
                BlogId = dbSample.BlogId,
                Content = dbSample.Content,
                Title = dbSample.Title
            };
        }

        public async Task<SampleDto> AddSample(SampleDto dto)
        {
            var dbSample = new DbContext.Entities.Sample
            {
                BlogId = dto.BlogId,
                Content = dto.Content,
                Title = dto.Title
            };

            await _dbContext.Samples.AddAsync(dbSample);
            await _dbContext.SaveChangesAsync();

            dto.Id = dbSample.Id;

            return dto;
        }

        public async Task<bool> UpdateSample(SampleDto dto)
        {
            var dbSample = await _dbContext.Samples.FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (dbSample == null)
                return false;

            dbSample.Id = dto.Id;
            dbSample.BlogId = dto.BlogId;
            dbSample.Content = dto.Content;
            dbSample.Title = dto.Title;
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
