using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra;

public sealed class DbInitializer(AppDbContext dbContext) : IDbInitializer
{
    public async Task Initialize()
    {
        await dbContext.Database.MigrateAsync();

        await Seed();
    }

    private async Task Seed()
    {
        foreach (var keyValue in SharedConstants.TtsApis)
        {
            if (!dbContext.TtsApis.Any(s => s.Id == keyValue.Value))
            {
                var service = new TtsApi()
                {
                    Name = keyValue.Key,
                    Id = keyValue.Value,
                };

                await dbContext.TtsApis.AddAsync(service);
            }
        }

        if (!HostingEnvironment.IsDevelopment())
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        var audios = new List<AudioFile>()
        {
            TestData.CreateAudioSampleAlloy(),
            TestData.CreateAudioFullFable()
        };

        foreach (var audio in audios)
        {
            if (!dbContext.AudioFiles.Any(a => a.Id == audio.Id))
            {
                await dbContext.AudioFiles.AddAsync(audio);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
