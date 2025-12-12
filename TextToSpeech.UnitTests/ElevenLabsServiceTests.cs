using ElevenLabs;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Infra.Services.Ai;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class ElevenLabsServiceTests
{
    [Fact]
    public async Task GetVoices_ReturnsMappedVoices()
    {
        var service = CreateService();

        var result = await service.GetVoices();

        Assert.NotNull(result);
        Assert.Collection(result,
            v =>
            {
                Assert.Equal("Voice A", v.Name);
                Assert.Equal("v-1", v.ProviderVoiceId);
            },
            v =>
            {
                Assert.Equal("Voice B", v.Name);
                Assert.Equal("v-2", v.ProviderVoiceId);
            });
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ReturnsEmpty_WhenNoChunks()
    {
        var service = CreateService();

        var result = await service.RequestSpeechChunksAsync([], Guid.NewGuid(), TestData.TtsRequestOptions, Mocks.ProgressCallback, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_ThrowsWhenCanceled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var service = CreateService();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            service.RequestSpeechChunksAsync(["text"], Guid.NewGuid(), TestData.TtsRequestOptions, Mocks.ProgressCallback, cts.Token));
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_Reports100PercentPerChunk()
    {
        var fileId = Guid.NewGuid();
        var progressContext = Mocks.CreateProgressContext(fileId);

        var service = CreateService(progressContext);

        var textChunks = new List<string> { "first", "second" };

        var result = await service.RequestSpeechChunksAsync(textChunks, fileId, TestData.TtsRequestOptions,
            progressContext.Progress, CancellationToken.None);

        Assert.Equal(textChunks.Count, result.Length);
        Assert.All(result, bytes => Assert.False(bytes.IsEmpty));
        Assert.Equal(new[] { 100, 100 }, progressContext.ReportedPercentages);

        progressContext.TrackerMock.Verify(p => p.InitializeFile(fileId, textChunks.Count), Times.Once);
        progressContext.TrackerMock.Verify(p => p.UpdateProgress(fileId, progressContext.Progress, It.IsAny<int>(), 100), Times.Exactly(textChunks.Count));
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_UsesParallelExecution()
    {
        var chunks = new List<string> { "chunk-1", "chunk-2", "chunk-3" };
        var fileId = Guid.NewGuid();

        IReadOnlyList<string>? capturedItems = null;
        int capturedMaxParallel = -1;
        Func<string, int, Task>? capturedAction = null;
        CancellationToken capturedToken = default;

        var parallelExecutionServiceMock = new Mock<IParallelExecutionService>();

        parallelExecutionServiceMock
            .Setup(x => x.RunTasksFromItems(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<int>(),
                It.IsAny<Func<string, int, Task>>(),
                It.IsAny<CancellationToken>()))
            .Callback((IReadOnlyList<string> items,
                    int maxParallel,
                    Func<string, int, Task> action,
                    CancellationToken token) =>
            {
                capturedItems = items;
                capturedMaxParallel = maxParallel;
                capturedAction = action;
                capturedToken = token;
            })
            .Returns(Task.CompletedTask);

        var service = CreateService(parallelExecutionServiceMock: parallelExecutionServiceMock);

        var result = await service.RequestSpeechChunksAsync(chunks, fileId, TestData.TtsRequestOptions,
            Mocks.ProgressCallback, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(chunks.Count, result.Length);
        Assert.Same(chunks, capturedItems);
        Assert.Equal(20, capturedMaxParallel);
        Assert.NotNull(capturedAction);
        Assert.Equal(CancellationToken.None, capturedToken);
    }

    private static ElevenLabsService CreateService(ProgressTrackerContext? progressContext = null,
        Mock<IParallelExecutionService>? parallelExecutionServiceMock = null)
    {
        var handler = FakeElevenLabsHandler.WithResponses();

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = handler.BaseAddress
        };

        var elevenLabsClient = new ElevenLabsClient(
            new ElevenLabsAuthentication("test-key"),
            new ElevenLabsClientSettings(handler.BaseAddress.ToString()),
            httpClient);

        progressContext ??= Mocks.CreateProgressContext(Guid.NewGuid());

        return new ElevenLabsService(
            elevenLabsClient,
            Mock.Of<ILogger<ElevenLabsService>>(),
            progressContext.TrackerMock.Object,
            parallelExecutionServiceMock?.Object ?? Mocks.ParallelExecutionService);
    }

    private sealed class FakeElevenLabsHandler : HttpMessageHandler
    {
        private readonly byte[] _audioBytes;
        private readonly string _voicesResponse;

        public Uri BaseAddress { get; } = new("https://fake.elevenlabs.local/");

        private FakeElevenLabsHandler(string voicesResponse, byte[] audioBytes)
        {
            _voicesResponse = voicesResponse;
            _audioBytes = audioBytes;
        }

        public static FakeElevenLabsHandler WithResponses(byte[]? audioBytes = null)
        {
            const string defaultVoicesResponse = """
            {
              "voices": [
                { "voice_id": "v-1", "name": "Voice A" },
                { "voice_id": "v-2", "name": "Voice B" }
              ]
            }
            """;

            return new FakeElevenLabsHandler(defaultVoicesResponse, audioBytes ?? [1, 2, 3]);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath.Contains("voices", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_voicesResponse, Encoding.UTF8, "application/json")
                });
            }

            if (request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath.Contains("text-to-speech", StringComparison.OrdinalIgnoreCase))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(_audioBytes)
                };

                response.Headers.TryAddWithoutValidation("history-item-id", "history-item-id-1");

                return Task.FromResult(response);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
