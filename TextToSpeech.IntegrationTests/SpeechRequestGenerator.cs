using Bogus;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using TextToSpeech.Api;
using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Constants;

namespace TextToSpeech.IntegrationTests;

internal static class SpeechRequestGenerator
{
    public static SpeechRequest GenerateFakeSpeechRequest(string ttsApi, bool addFile = false)
    {
        var faker = new Faker();

        // build fake TTS options first
        var ttsOptions = new TtsRequestOptions
        {
            Model = ttsApi == SharedConstants.OpenAI ? "gpt-4o-mini-tts" : null,
            Voice = ttsApi == SharedConstants.OpenAI
                ? faker.PickRandom(new[] { "alloy", "echo", "fable", "onyx", "nova", "shimmer" })
                : "charles",
            Speed = Math.Round(faker.Random.Double(0.5, 2.0), 1),
            ResponseFormat = SpeechResponseFormat.Mp3
        };

        var speechRequest = new SpeechRequest
        {
            TtsApi = ttsApi,
            LanguageCode = "en",
            Input = faker.Lorem.Sentence(),
            TtsRequestOptions = ttsOptions,
            File = addFile ? CreateFakeFile() : null
        };

        return speechRequest;
    }

    private static IFormFile CreateFakeFile()
    {
        const string content = "Fake file content";
        const string fileName = "fakefile.txt";

        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.ContentType).Returns("text/plain");

        return fileMock.Object;
    }
}
