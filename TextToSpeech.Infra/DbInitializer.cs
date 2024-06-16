using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.Services.FileProcessing;
using static TextToSpeech.Core.Enums;

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
            CreateAudioSampleAlloy(),
            CreateAudioFullFable()
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

    public static AudioFile CreateAudioSampleAlloy()
    {
        var audio = AudioFileBuilder.Create(AudioFileService.GenerateSilentMp3(5),
            "Test sample audio file alloy",
            "alloy",
            "en",
            1,
            AudioType.Sample,
            "test_sample_audio_alloy.mp3",
            Guid.Parse("1bf4b7a2-b16f-401f-9a6c-42294eb3ea54"),
            SharedConstants.TtsApis[SharedConstants.OpenAI]);

        audio.Status = Status.Completed;

        return audio;
    }

    public static AudioFile CreateAudioFullFable()
    {
        var audio = AudioFileBuilder.Create("  "u8.ToArray(),
            "Test full audio file fable",
            "fable",
            "en",
            1,
            AudioType.Full,
            "test_full_audio_fable.mp3",
            Guid.Parse("b43cbe4a-806d-47a4-982e-0cb25df3e56c"),
            SharedConstants.TtsApis[SharedConstants.OpenAI]);

        audio.Status = Status.Completed;

        return audio;
    }
}
