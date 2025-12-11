using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using OpenAI.Audio;
using System.ClientModel;
using System.ClientModel.Primitives;
using TextToSpeech.Infra.Services.Ai;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class OpenAiServiceTests
{
    [Fact]
    public async Task GetVoices_ReturnsStaticVoiceList()
    {
        // Arrange
        var service = CreateSimpleOpenAiService();

        // Act
        var voices = await service.GetVoices();

        // Assert
        Assert.NotNull(voices);
        Assert.True(voices.Count >= 11);
        Assert.Contains(voices, v => v.Name == "Alloy" && v.ProviderVoiceId == "alloy");
        Assert.Contains(voices, v => v.Name == "Nova" && v.ProviderVoiceId == "nova");
        Assert.Contains(voices, v => v.Name == "Onyx" && v.ProviderVoiceId == "onyx");
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ReturnsEmptyArray_WhenNoTextChunks()
    {
        // Arrange
        var service = CreateSimpleOpenAiService();

        // Act
        var result = await service.RequestSpeechChunksAsync([], Guid.NewGuid(), TestData.TtsRequestOptions,
            Mocks.ProgressCallback, default);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ThrowsTaskCanceled_WhenTokenCanceledBeforeWork()
    {
        // Arrange
        var service = CreateSimpleOpenAiService();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            service.RequestSpeechChunksAsync(["text"], Guid.NewGuid(), TestData.TtsRequestOptions, Mocks.ProgressCallback, cts.Token));
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_Reports100PercentPerChunk()
    {
        var fileId = Guid.NewGuid();
        var progressContext = Mocks.CreateProgressContext(fileId);
        var logger = new Mock<ILogger<OpenAiService>>();

        var service = new OpenAiService(CreateOpenAIClientMock(), logger.Object, progressContext.TrackerMock.Object);

        var textChunks = new List<string> { "first", "second" };

        var result = await service.RequestSpeechChunksAsync(textChunks, fileId, TestData.TtsRequestOptions, progressContext.Progress, CancellationToken.None);

        Assert.Equal(textChunks.Count, result.Length);
        Assert.All(result, bytes => Assert.False(bytes.IsEmpty));
        Assert.Equal(new[] { 100, 100 }, progressContext.ReportedPercentages);

        progressContext.TrackerMock.Verify(p => p.InitializeFile(fileId, textChunks.Count), Times.Once);
        progressContext.TrackerMock.Verify(p => p.UpdateProgress(fileId, progressContext.Progress, It.IsAny<int>(), 100), Times.Exactly(textChunks.Count));
    }

    private static OpenAiService CreateSimpleOpenAiService()
    {
        var client = new OpenAIClient("test-key");
        var logger = new Mock<ILogger<OpenAiService>>();
        var service = new OpenAiService(client, logger.Object, Mocks.ProgressTracker);

        return service;
    }

    private static OpenAIClient CreateOpenAIClientMock()
    {
        var mockResult = new Mock<ClientResult<BinaryData>>(BinaryData.FromBytes([1, 2, 3]), Mock.Of<PipelineResponse>());

        mockResult.SetupGet(r => r.Value).Returns(BinaryData.FromBytes([1, 2, 3]));

        var audioClientMock = new Mock<AudioClient>();

        audioClientMock
            .Setup(c => c.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<GeneratedSpeechVoice>(),
                It.IsAny<SpeechGenerationOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult.Object);

        var openAiClientMock = new Mock<OpenAIClient>();

        openAiClientMock
            .Setup(c => c.GetAudioClient(It.IsAny<string>()))
            .Returns(audioClientMock.Object);

        return openAiClientMock.Object;
    }
}
