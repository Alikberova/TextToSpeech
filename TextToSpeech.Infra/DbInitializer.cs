using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Constants;

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
            var existing = await dbContext.AudioFiles.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == audio.Id);

            if (existing is null)
            {
                await dbContext.AudioFiles.AddAsync(audio);
            }
            else if (existing.Hash != audio.Hash)
            {
                dbContext.AudioFiles.Update(audio);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
