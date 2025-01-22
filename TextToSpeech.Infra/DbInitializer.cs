using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra;

public sealed class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _dbContext;

    public DbInitializer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Initialize()
    {
        await _dbContext.Database.MigrateAsync();

        await Seed();
    }

    private async Task Seed()
    {
        foreach (var keyValue in SharedConstants.TtsApis)
        {
            if (!_dbContext.TtsApis.Any(s => s.Id == keyValue.Value))
            {
                var service = new TtsApi()
                {
                    Name = keyValue.Key,
                    Id = keyValue.Value,
                };

                await _dbContext.TtsApis.AddAsync(service);
            }
        }

        if (!HostingEnvironment.IsDevelopment())
        {
            await _dbContext.SaveChangesAsync();
            return;
        }

        var audios = new List<AudioFile>()
        {
            TestData.CreateAudioSampleAlloy(),
            TestData.CreateAudioFullFable()
        };

        foreach (var audio in audios)
        {
            if (!_dbContext.AudioFiles.Any(a => a.Id == audio.Id))
            {
                await _dbContext.AudioFiles.AddAsync(audio);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
