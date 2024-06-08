using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces;
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

    public AudioFile CreateAudioSampleAlloy()
    {
        var audio = new AudioFile
        {
            Id = Guid.Parse("497540fb-2c84-449e-aadf-505b21aa0f69"),
            FileName = "test_sample_audio_alloy.mp3",
            Data = AudioFileService.GenerateSilentMp3(5),
            CreatedAt = DateTime.UtcNow,
            Description = "Test sample audio file alloy",
            Status = Status.Completed,
            Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(SharedConstants.AngularDemoText))),
            Voice = "alloy",
            LanguageCode = "en",
            Speed = 1,
            Type = AudioType.Sample,
            TtsApiId = SharedConstants.TtsApis[SharedConstants.OpenAI]
        };

        return audio;
    }

    public static AudioFile CreateAudioFullFable()
    {
        var audio = new AudioFile
        {
            Id = Guid.Parse("95c53f80-63d5-40c2-96f1-0a38d978f77f"),
            FileName = "test_full_audio_fable.mp3",
            Data = "  "u8.ToArray(),
            CreatedAt = DateTime.UtcNow,
            Description = "Test full audio file fable",
            Status = Status.Completed,
            Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(SharedConstants.FullAudioFileContentForTesting))),
            Voice = "fable",
            LanguageCode = "en",
            Speed = 1,
            Type = AudioType.Full,
            TtsApiId = SharedConstants.TtsApis[SharedConstants.OpenAI]
        };

        return audio;
    }
}
