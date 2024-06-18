using Moq;
using TextToSpeech.Core.Interfaces.Repositories;

namespace TextToSpeech.TestingInfra.Mocks;

public static class IAudioFileRepositoryMock
{
    public static Mock<IAudioFileRepository> Get()
    {
        var mock = new Mock<IAudioFileRepository>();

        mock.Setup(s =>
            s.GetAudioFileByHashAsync(
                It.IsAny<string>()));

        return mock;
    }
}
