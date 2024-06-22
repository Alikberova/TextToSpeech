using Moq;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Infra.Services.FileProcessing;

namespace TextToSpeech.TestingInfra.Mocks;

internal static class ITtsServiceMock
{
    public static Mock<ITtsService> Get()
    {
        var mockOpenAiService = new Mock<ITtsService>();

        mockOpenAiService.SetupGet(service =>
            service.MaxLengthPerApiRequest)
                .Returns(150);

        mockOpenAiService.Setup(service =>
            service.RequestSpeechChunksAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<string>()))
            .ReturnsAsync([new ReadOnlyMemory<byte>(AudioFileService.GenerateSilentMp3(3))]);

        return mockOpenAiService;
    }
}
