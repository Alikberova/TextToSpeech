using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Infra.Services.Interfaces;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Infra;

public sealed class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly IAudioFileService _audioFileService;

    public DbInitializer(AppDbContext dbContext, IAudioFileService audioFileService)
    {
        _dbContext = dbContext;
        _audioFileService = audioFileService;
    }

    public void Initialize()
    {
        _dbContext.Database.Migrate();

        Seed();
    }

    private void Seed()
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

                _dbContext.TtsApis.Add(service);
            }
        }

        if (!HostingEnvironment.IsDevelopment())
        {
            return;
        }

        // todo if debug
        var audios = new List<AudioFile>()
        {
            CreateAudioSampleAlloy(),
            CreateAudioFullFable()
        };

        foreach (var audio in audios)
        {
            if (!_dbContext.AudioFiles.Any(a => a.Id == audio.Id))
            {
                _dbContext.AudioFiles.Add(audio);
            }
        }

        _dbContext.SaveChanges();
    }

    public AudioFile CreateAudioSampleAlloy()
    {
        var audio = new AudioFile
        {
            Id = Guid.Parse("497540fb-2c84-449e-aadf-505b21aa0f69"),
            FileName = "test_sample_audio_alloy.mp3",
            Data = _audioFileService.GenerateSilentMp3(5),
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
