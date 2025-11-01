using static TextToSpeech.Core.Enums;
using TextToSpeech.Core.Config;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Infra.Services;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Models;

namespace TextToSpeech.Infra;

public static class TestData
{
    public static AudioFile CreateAudioSampleAlloy()
    {
        var audio = AudioFileBuilder.Create(AudioFileService.GenerateSilentMp3(5),
            "en",
            AudioType.Sample,
            SharedConstants.AngularDemoText,
            new TtsRequestOptions()
            {
                Voice = "alloy",
                Speed = 1,
                ResponseFormat = SpeechResponseFormat.Mp3
            },
            SharedConstants.TtsApis[SharedConstants.OpenAI],
            "test_sample_audio_alloy.mp3",
            Guid.Parse("32f7811a-0cc5-49d7-b0e1-8417cc08d78f"));

        audio.Status = Status.Completed;

        return audio;
    }

    public static AudioFile CreateAudioFullFable()
    {
        var audio = AudioFileBuilder.Create("  "u8.ToArray(),
            "en",
            AudioType.Full,
            SharedConstants.FullAudioFileContentForTesting,
            new TtsRequestOptions()
            {
                Voice = "fable",
                Speed = 1,
                ResponseFormat = SpeechResponseFormat.Mp3
            },
            SharedConstants.TtsApis[SharedConstants.OpenAI],
            "test_full_audio_fable.mp3",
            Guid.Parse("c3c54b87-21c4-43a4-a774-2b7646484edd"));

        audio.Status = Status.Completed;

        return audio;
    }
}
