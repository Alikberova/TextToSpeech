using TextToSpeech.Core.Config;
using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.Services.FileProcessing;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Infra;

public static class TestData
{
    public static class TextToSpeechFormConstants
    {
        public const string English = "English";
        public const string GermanStandard = "German (Standard)";
        public const string NarakeetVoiceHans = "Hans";
        public const string NarakeetVoiceAmanda = "Amanda";
        public const string OpenAiVoiceFable = "Fable";
        public const string OpenAiVoiceAlloy = "Alloy";
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
                Voice = TextToSpeechFormConstants.OpenAiVoiceAlloy.ToLower(),
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
                Voice = TextToSpeechFormConstants.OpenAiVoiceFable.ToLower(),
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

    public static List<VoiceResponse> GetVoicesNarakeet()
    {
        var voices = new List<VoiceResponse>
        {
            new() {
                Name = "anders",
                Language = "Danish",
                LanguageCode = "da-DK",
                Styles = []
            },
            new() {
                Name = "amanda",
                Language = TextToSpeechFormConstants.English,
                LanguageCode = "en-US",
                Styles = []
            },
            new() {
                Name = "armin",
                Language = TextToSpeechFormConstants.GermanStandard,
                LanguageCode = "de-DE",
                Styles = []
            },
            new() {
                Name = TextToSpeechFormConstants.NarakeetVoiceHans,
                Language = TextToSpeechFormConstants.GermanStandard,
                LanguageCode = "de-DE",
                Styles = []
            },
        };

        // .ToLowerInvariant() as it comes from api in low case

        return [.. voices.Select(v => v with { Name = v.Name.ToLowerInvariant() })];
    }
}
