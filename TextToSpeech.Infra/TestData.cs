using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.Services.FileProcessing;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Infra;

public static class TestData
{
    public static class Lang
    {
        public const string English = "English";
        public const string GermanStandard = "German (Standard)";
    }

    public const string TtsSampleRequest =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed 11a47387-8d4a-4956-9a1e-352628301dab";
    public const string TtsFullRequest = "Test content for audio file full type";

    public static AudioFile CreateAudioSampleAlloy()
    {
        var audio = AudioFileBuilder.Create(AudioFileService.GenerateSilentMp3(5),
            string.Empty,
            AudioType.Sample,
            TtsSampleRequest,
            new TtsRequestOptions()
            {
                Voice = OpenAiVoices.Alloy.ProviderVoiceId,
                Model = "tts-1",
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
        var audio = AudioFileBuilder.Create(AudioFileService.GenerateSilentMp3(3),
            string.Empty,
            AudioType.Full,
            TtsFullRequest,
            new TtsRequestOptions()
            {
                Voice = OpenAiVoices.Fable.ProviderVoiceId,
                Model = "tts-1",
                Speed = 1,
                ResponseFormat = SpeechResponseFormat.Mp3
            },
            SharedConstants.TtsApis[SharedConstants.OpenAI],
            "test_full_audio_fable.mp3",
            Guid.Parse("c3c54b87-21c4-43a4-a774-2b7646484edd"));

        audio.Status = Status.Completed;

        return audio;
    }

    public static class OpenAiVoices
    {
        public static Voice[] All => [Alloy, Fable];

        public static Voice Alloy => new()
        {
            Name = "Alloy",
            ProviderVoiceId = "alloy",
        };

        public static Voice Fable => new()
        {
            Name = "Fable",
            ProviderVoiceId = "fable",
        };
    }

    public static class NarakeetVoices
    {
        public static Voice[] All => [Hans, Armin, Amanda, Anders];

        public static Voice Hans => new()
        {
            Name = "Hans",
            ProviderVoiceId = "hans",
            Language = new Language(Lang.GermanStandard, "de-DE"),
        };

        public static Voice Armin => new()
        {
            Name = "Armin",
            ProviderVoiceId = "armin",
            Language = new Language(Lang.GermanStandard, "de-DE"),
        };

        public static Voice Amanda => new()
        {
            Name = "Amanda",
            ProviderVoiceId = "amanda",
            Language = new Language(Lang.English, "en-US"),
        };

        public static Voice Anders => new()
        {
            Name = "Anders",
            ProviderVoiceId = "anders",
            Language = new Language("Danish", "da-DK"),
        };
    }
}
