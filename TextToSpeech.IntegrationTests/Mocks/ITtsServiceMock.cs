using Moq;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Services.FileProcessing;

namespace TextToSpeech.IntegrationTests.Mocks;

internal static class ITtsServiceMock
{
    public static Mock<ITtsService> Get()
    {
        var mockOpenAiService = new Mock<ITtsService>();

        mockOpenAiService.SetupGet(service =>
            service.MaxLengthPerApiRequest)
                .Returns(150);

        mockOpenAiService
            .Setup(service =>
                service.RequestSpeechChunksAsync(
                    It.IsAny<List<string>>(),
                    It.IsAny<Guid>(),
                    It.IsAny<TtsRequestOptions>(),
                    It.IsAny<IProgress<ProgressReport>>(),
                    It.IsAny<CancellationToken>()))
            .Returns((List<string> textChunks,
                Guid fileId,
                TtsRequestOptions options,
                IProgress<ProgressReport>? progress,
                CancellationToken cancellationToken) =>
                {
                    return Task.Run(() =>
                    {
                        // Simulate progress updates
                        for (int i = 0; i <= 100; i += 10)
                        {
                            progress?.Report(new ProgressReport
                            {
                                FileId = fileId,
                                ProgressPercentage = i
                            });
                            Task.Delay(100).Wait();  // Simulate work being done
                        }
                        return GetAudioBytesArray();
                    });
                });

        mockOpenAiService
            .Setup(s => s.RequestSpeechSample(
                It.IsAny<string>(),
                It.IsAny<TtsRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AudioFileService.GenerateSilentMp3(2));

        return mockOpenAiService;
    }

    private static ReadOnlyMemory<byte>[] GetAudioBytesArray()
    {
        const int length = 3;
        var bytesArray = new ReadOnlyMemory<byte>[length];

        for (int i = 0; i < length; i++)
        {
            bytesArray[i] = AudioFileService.GenerateSilentMp3(2);
        }

        return bytesArray;
    }
}
