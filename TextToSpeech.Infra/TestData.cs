using System.Text;
using static TextToSpeech.Core.Enums;
using TextToSpeech.Core.Config;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Infra.Services;
using TextToSpeech.Core.Entities;

namespace TextToSpeech.Infra;

public static class TestData
{
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
        audio.Hash = AudioFileBuilder.GenerateAudioFileHash(Encoding.UTF8.GetBytes(GetTestFullFableContent()),
            audio.Voice, audio.LanguageCode, audio.Speed);

        return audio;
    }

    public static string GetTestFullFableContent()
    {
        return CreateString(SharedConstants.FullAudioFileContentForTesting, 20000);
    }

    private static string CreateString(string content, int repetitions)
    {
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < repetitions; i++)
        {
            stringBuilder.Append(content);
        }

        return stringBuilder.ToString();
    }
}
