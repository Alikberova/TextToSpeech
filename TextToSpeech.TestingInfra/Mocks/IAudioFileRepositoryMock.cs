using Moq;
using TextToSpeech.Core.Repositories;

namespace TextToSpeech.TestingInfra.Mocks;

public static class IAudioFileRepositoryMock
{
    public static Mock<IAudioFileRepository> Get()
    {
        var mock = new Mock<IAudioFileRepository>();

        mock.Setup(s =>
            s.GetAudioFileAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<double>()));

        return mock;
    }
}
