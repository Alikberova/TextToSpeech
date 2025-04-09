using Bogus;
using TextToSpeech.Core.Config;
using Microsoft.AspNetCore.Http;
using Moq;
using OpenAI.Audio;
using System.Text;

using SpeechRequest = TextToSpeech.Core.Dto.SpeechRequest;

namespace TextToSpeech.TestingInfra.DataGenerators;

public class SpeechRequestGenerator
{
    public static SpeechRequest GenerateFakeSpeechRequest(string ttsApi, bool addFile = false)
    {
        var faker = new Faker<SpeechRequest>()
            .RuleFor(o => o.Model, f => ttsApi is SharedConstants.OpenAI ? "tts-1" : string.Empty)
            .RuleFor(o => o.TtsApi, f => ttsApi)
            .RuleFor(o => o.Speed, f => Math.Round(f.Random.Double(0.5, 2), 1))
            .RuleFor(o => o.Input, f => f.Lorem.Sentence());

        if (addFile)
        {
            faker.RuleFor(o => o.File, _ => CreateFakeFile());
        }

        if (ttsApi is SharedConstants.OpenAI)
        {
            faker.RuleFor(o => o.Voice, f => f.PickRandom(Enum.GetNames(typeof(SpeechVoice))));
        }

        if (ttsApi is SharedConstants.Narakeet)
        {
            faker.RuleFor(o => o.Voice, _ => "charles");

        }

        var speechRequest = faker.Generate();

        return speechRequest;
    }

    private static IFormFile CreateFakeFile()
    {
        const string content = "Fake file content";
        const string fileName = "fakefile.txt";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.OpenReadStream()).Returns(stream);
        fileMock.Setup(_ => _.Length).Returns(stream.Length);

        return fileMock.Object;
    }
}