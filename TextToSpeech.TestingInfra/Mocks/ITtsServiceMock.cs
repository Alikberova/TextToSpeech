using Moq;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.TestingInfra.Mocks;

internal static class ITtsServiceMock
{
    public static Mock<ITtsService> Get()
    {
        var mockOpenAiService = new Mock<ITtsService>();

        mockOpenAiService.SetupGet(service =>
            service.MaxInputLength)
                .Returns(150);

        mockOpenAiService.Setup(service =>
            service.RequestSpeechChunksAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<string>()))
            .ReturnsAsync([new ReadOnlyMemory<byte>(CreateMockMp3Data())]);

        return mockOpenAiService;
    }

    private static byte[] CreateMockMp3Data()
    {
        // MP3 files usually start with the 'ID3' tag followed by more data.
        // This is a simplistic representation and might not be playable.
        var mp3Header = new byte[] { 0x49, 0x44, 0x33 }; // 'ID3' in bytes
        var mp3Data = new byte[100];
        mp3Header.CopyTo(mp3Data, 0);

        return mp3Data;
    }
}
