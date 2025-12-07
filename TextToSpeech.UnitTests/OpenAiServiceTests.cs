using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using TextToSpeech.Infra.Services.Ai;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class OpenAiServiceTests
{
    [Fact]
    public async Task GetVoices_ReturnsStaticVoiceList()
    {
        // Arrange
        var service = CreateOpenAiService();

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
        var service = CreateOpenAiService();

        // Act
        var result = await service.RequestSpeechChunksAsync([], Guid.NewGuid(), TestData.TtsRequestOptions);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ThrowsTaskCanceled_WhenTokenCanceledBeforeWork()
    {
        // Arrange
        var service = CreateOpenAiService();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            service.RequestSpeechChunksAsync(["text"], Guid.NewGuid(), TestData.TtsRequestOptions, cancellationToken: cts.Token));
    }

    private static OpenAiService CreateOpenAiService()
    {
        var client = new OpenAIClient("test-key");
        var logger = new Mock<ILogger<OpenAiService>>();
        var service = new OpenAiService(client, logger.Object);

        return service;
    }
}
