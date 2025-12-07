using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using TextToSpeech.Infra.Dto;
using TextToSpeech.Infra.Services.Ai;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class NarakeetServiceTests
{
    [Fact]
    public async Task GetVoices_ReturnsMappedVoices_WhenApiResponds()
    {
        // Arrange
        var apiResult = new List<NarakeetVoiceResult>
        {
            new() { Name = "voice-a", Language = "en" },
            new() { Name = "voice-b", Language = "fr" }
        };

        var (service, _) = CreateNarakeetService(apiResult);

        // Act
        var voices = await service.GetVoices();

        // Assert
        Assert.NotNull(voices);
        Assert.Collection(voices,
            v =>
            {
                Assert.Equal("voice-a", v.Name);
                Assert.Equal("voice-a", v.ProviderVoiceId);
                Assert.Equal("en", v.Language);
            },
            v =>
            {
                Assert.Equal("voice-b", v.Name);
                Assert.Equal("voice-b", v.ProviderVoiceId);
                Assert.Equal("fr", v.Language);
            });
    }

    [Fact]
    public async Task GetVoices_ReturnsNull_WhenApiReturnsEmptyList()
    {
        // Arrange
        var apiResult = new List<NarakeetVoiceResult>();

        var (service, _) = CreateNarakeetService(apiResult);

        // Act
        var voices = await service.GetVoices();

        // Assert
        Assert.Null(voices);
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ReturnsEmptyArray_WhenNoTextChunks()
    {
        // Arrange
        var (service, handler) = CreateNarakeetService([], MockBehavior.Strict);

        // Act
        var result = await service.RequestSpeechChunksAsync([], Guid.NewGuid(), TestData.TtsRequestOptions);

        // Assert
        Assert.Empty(result);

        handler.Protected().Verify("SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ThrowsOperationCanceled_WhenTokenCanceledBeforeRequests()
    {
        // Arrange
        var (service, handler) = CreateNarakeetService([], MockBehavior.Strict);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.RequestSpeechChunksAsync(["text"], Guid.NewGuid(), TestData.TtsRequestOptions, cancellationToken: cts.Token));

        handler.Protected().Verify("SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    private static (NarakeetService service, Mock<HttpMessageHandler> handler) CreateNarakeetService(
        List<NarakeetVoiceResult> apiResult,
        MockBehavior behavior = MockBehavior.Loose)
    {
        var httpHandler = new Mock<HttpMessageHandler>(behavior);

        httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(apiResult)
            });

        var httpClient = new HttpClient(httpHandler.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        var service = new NarakeetService(httpClient);

        return (service, httpHandler);
    }
}
