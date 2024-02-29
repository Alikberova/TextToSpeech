using Bogus;
using Microsoft.AspNetCore.Http;
using Moq;
using OpenAI.Audio;
using System.Text;

using SpeechRequest = BookToAudio.Core.Dto.SpeechRequest;

namespace BookToAudio.TestingInfra.DataGenerators;

public class SpeechRequestGenerator
{
    public static SpeechRequest GenerateFakeSpeechRequest(bool addFile = false)
    {
        // todo tts-1 to const
        var faker = new Faker<SpeechRequest>()
            .RuleFor(o => o.Model, f => "tts-1")
            .RuleFor(o => o.Voice, f => f.PickRandom(Enum.GetValues(typeof(SpeechVoice)).Cast<SpeechVoice>().ToArray()))
            .RuleFor(o => o.Speed, f => f.Random.Float(0.5f, 2.0f))
            .RuleFor(o => o.Input, f => f.Lorem.Sentence());
            
        if (addFile)
        {
            faker.RuleFor(o => o.File, _ => CreateFakeFile());
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