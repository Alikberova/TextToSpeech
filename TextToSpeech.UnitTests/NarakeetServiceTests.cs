using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TextToSpeech.Infra.Dto.Narakeet;
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
            new() { Name = "voice-a", Language = "English", LanguageCode = "en" },
            new() { Name = "voice-b", Language = "French", LanguageCode = "fr" }
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
                Assert.Equal("en", v.Language?.LanguageCode);
                Assert.Equal("English", v.Language?.Name);
            },
            v =>
            {
                Assert.Equal("voice-b", v.Name);
                Assert.Equal("voice-b", v.ProviderVoiceId);
                Assert.Equal("fr", v.Language?.LanguageCode);
                Assert.Equal("French", v.Language?.Name);
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
        var result = await service.RequestSpeechChunksAsync([], Guid.NewGuid(), TestData.TtsRequestOptions, Mocks.ProgressCallback, default);

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
            service.RequestSpeechChunksAsync(["text"], Guid.NewGuid(), TestData.TtsRequestOptions, Mocks.ProgressCallback, cts.Token));

        handler.Protected().Verify("SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ReportsInternalPercentToTracker()
    {
        var fileId = Guid.NewGuid();
        var progressContext = Mocks.CreateProgressContext(fileId);
        var buildTaskResponse = new BuildTask { StatusUrl = "https://example.com/status", TaskId = "task-id", RequestId = "request-id" };
        var inProgressPercent = 45;
        var inProgressStatus = new BuildTaskStatus { Finished = false, Percent = inProgressPercent, Result = string.Empty, Message = "working", Succeeded = false };
        var finishedStatus = new BuildTaskStatus { Finished = true, Percent = 100, Result = "https://example.com/result", Message = "done", Succeeded = true };

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(buildTaskResponse))
            });

        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == buildTaskResponse.StatusUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(inProgressStatus))
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(finishedStatus))
            });

        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == finishedStatus.Result),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent([1, 2, 3])
            });

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        var logger = new Mock<ILogger<NarakeetService>>();
        var service = new NarakeetService(httpClient, progressContext.TrackerMock.Object, logger.Object);

        var result = await service.RequestSpeechChunksAsync(["chunk"], fileId, TestData.TtsRequestOptions, progressContext.Progress, default);

        Assert.Single(result);
        Assert.False(result[0].IsEmpty);
        progressContext.TrackerMock.Verify(t => t.InitializeFile(fileId, 1), Times.Once);
        progressContext.TrackerMock.Verify(t => t.UpdateProgress(fileId, progressContext.Progress, 0, inProgressPercent), Times.Once);
        Assert.Contains(inProgressPercent, progressContext.ReportedPercentages);
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

        var logger = new Mock<ILogger<NarakeetService>>();

        var service = new NarakeetService(httpClient, Mocks.ProgressTracker, logger.Object);

        return (service, httpHandler);
    }
}