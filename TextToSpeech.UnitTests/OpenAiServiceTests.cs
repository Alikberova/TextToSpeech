using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using OpenAI.Audio;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
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
        var result = await service.RequestSpeechChunksAsync([], Guid.NewGuid(), TestData.TtsRequestOptions,
            Mocks.ProgressCallback, default);

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
            service.RequestSpeechChunksAsync(["text"], Guid.NewGuid(), TestData.TtsRequestOptions, Mocks.ProgressCallback, cts.Token));
    }

    [Fact]
    public async Task RequestSpeechChunksAsync_Reports100PercentPerChunk()
    {
        var fileId = Guid.NewGuid();
        var progressContext = Mocks.CreateProgressContext(fileId);
        var client = new OpenAIClient("test-key");
        var logger = new Mock<ILogger<OpenAiService>>();
        var service = new OpenAiService(client, logger.Object, progressContext.TrackerMock.Object);

        SetAudioClient(service, CreateAudioClientMock().Object);

        var textChunks = new List<string> { "first", "second" };

        var result = await service.RequestSpeechChunksAsync(textChunks, fileId, TestData.TtsRequestOptions, progressContext.Progress, CancellationToken.None);

        Assert.Equal(textChunks.Count, result.Length);
        Assert.All(result, bytes => Assert.False(bytes.IsEmpty));
        Assert.Equal(new[] { 100, 100 }, progressContext.ReportedPercentages);

        progressContext.TrackerMock.Verify(p => p.InitializeFile(fileId, textChunks.Count), Times.Once);
        progressContext.TrackerMock.Verify(p => p.UpdateProgress(fileId, progressContext.Progress, It.IsAny<int>(), 100), Times.Exactly(textChunks.Count));
    }

    private static OpenAiService CreateOpenAiService()
    {
        var client = new OpenAIClient("test-key");
        var logger = new Mock<ILogger<OpenAiService>>();
        var service = new OpenAiService(client, logger.Object, Mocks.ProgressTracker);

        return service;
    }

    private static Mock<AudioClient> CreateAudioClientMock()
    {
        var audioClient = new Mock<AudioClient>("https://example.com", "fake-key");

        audioClient.Setup(c => c.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<GeneratedSpeechVoice>(),
                It.IsAny<SpeechGenerationOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestClientResult(BinaryData.FromBytes(new byte[] { 1, 2, 3 })));

        return audioClient;
    }

    private static Mock<AudioClient> CreateAudioClientMock1()
    {
        var audioClient = new Mock<AudioClient>("https://example.com", "fake-key");

        audioClient.Setup(c => c.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<GeneratedSpeechVoice>(),
                It.IsAny<SpeechGenerationOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestClientResult(BinaryData.FromBytes(new byte[] { 1, 2, 3 })));

        return audioClient;
    }

    private static void SetAudioClient(OpenAiService service, AudioClient audioClient)
    {
        var property = typeof(OpenAiService).GetProperty("AudioClient",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property!.SetValue(service, audioClient);
    }

    private sealed class TestClientResult : ClientResult<BinaryData>
    {
        public TestClientResult(BinaryData value) : base(value, new TestPipelineResponse())
        {
        }
    }

    private sealed class TestPipelineResponse : PipelineResponse
    {
        private Stream _contentStream = Stream.Null;

        public override int Status => 200;
        public override string ReasonPhrase => "OK";
        [AllowNull]
        public override Stream ContentStream
        {
            get => _contentStream;
            set => _contentStream = value ?? Stream.Null;
        }
        public override BinaryData Content => BinaryData.Empty;

        protected override PipelineResponseHeaders HeadersCore { get; } = new TestPipelineResponseHeaders();

        public override BinaryData BufferContent(CancellationToken cancellationToken = default) => Content;

        public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Content);

        public override void Dispose()
        {
        }
    }

    private sealed class TestPipelineResponseHeaders : PipelineResponseHeaders
    {
        private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _headers.GetEnumerator();

        public override bool TryGetValue(string name, out string? value) => _headers.TryGetValue(name, out value);

        public override bool TryGetValues(string name, out IEnumerable<string>? values)
        {
            if (_headers.TryGetValue(name, out var value))
            {
                values = new[] { value };
                return true;
            }

            values = null;
            return false;
        }
    }
}
