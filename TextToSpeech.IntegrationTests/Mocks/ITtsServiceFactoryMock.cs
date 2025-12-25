using Moq;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.IntegrationTests.Mocks;

public static class ITtsServiceFactoryMock // todo rm
{
    public static Mock<ITtsServiceFactory> Get()
    {
        var ttsfactory = new Mock<ITtsServiceFactory>();

        ttsfactory.Setup(service =>
            service.Get(It.IsAny<string>()))
            .Returns(ITtsServiceMock.Get().Object);

        return ttsfactory;
    }
}
