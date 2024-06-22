using Microsoft.EntityFrameworkCore;
using System.Text;
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
            "alloy",
            "en",
            1,
            AudioType.Sample,
            SharedConstants.TtsApis[SharedConstants.OpenAI],
            "test_sample_audio_alloy.mp3",
            Guid.Parse("32f7811a-0cc5-49d7-b0e1-8417cc08d78f"));

        audio.Status = Status.Completed;
        audio.Hash = AudioFileBuilder.GenerateAudioFileHash(Encoding.UTF8.GetBytes(SharedConstants.AngularDemoText),
            audio.Voice, audio.LanguageCode, audio.Speed);

        return audio;
    }

    public static AudioFile CreateAudioFullFable()
    {
        var audio = AudioFileBuilder.Create("  "u8.ToArray(),
            "fable",
            "en",
            1,
            AudioType.Full,
            SharedConstants.TtsApis[SharedConstants.OpenAI],
            "test_full_audio_fable.mp3",
            Guid.Parse("c3c54b87-21c4-43a4-a774-2b7646484edd"));

        audio.Status = Status.Completed;
        audio.Hash = AudioFileBuilder.GenerateAudioFileHash(Encoding.UTF8.GetBytes(SharedConstants.FullAudioFileContentForTesting),
            audio.Voice, audio.LanguageCode, audio.Speed);

        return audio;
    }
}
