using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// 50 requests/min for model tts-1
/// </summary>
public sealed class OpenAiService : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 4096;
    // Limit the number of parallel chunk requests to meet rate limits
    private const int MaxParallelChunks = 20;
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAiService> _logger;
    private readonly IProgressTracker _progressTracker;

    public OpenAiService(OpenAIClient client, ILogger<OpenAiService> logger, IProgressTracker progressTracker)
    {
        _client = client;
        _logger = logger;
        _progressTracker = progressTracker;
    }

    private AudioClient AudioClient { get; set; } = default!;

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        Guid fileId,
        TtsRequestOptions ttsRequest,
        IProgress<ProgressReport> progressCallback,
        CancellationToken cancellationToken)
    {
        var totalChunks = textChunks.Count;

        var results = new ReadOnlyMemory<byte>[totalChunks];

        _progressTracker.InitializeFile(fileId, totalChunks);

        using var gate = new SemaphoreSlim(MaxParallelChunks);

        _logger.LogInformation("Initialized SemaphoreSlim with max parallel chunk limit {Limit}",
            MaxParallelChunks);

        var tasks = textChunks.Select((chunk, index) =>
        {
            return Task.Run(async () =>
            {
                await gate.WaitAsync(cancellationToken);

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var bytes = await GenerateSpeech(chunk, ttsRequest.Voice, ttsRequest.Speed,
                        ttsRequest.ResponseFormat, cancellationToken);

                    results[index] = bytes;

                    var progress = _progressTracker.UpdateProgress(fileId, progressCallback, index, 100);

                    _logger.LogInformation("Processed chunk {ChunkIndex}/{TotalChunks} for file {FileId}. Progress: {Progress}%",
                        index + 1, totalChunks, fileId, progress);
                }
                finally
                {
                    gate.Release();
                }
            }, cancellationToken);
        }).ToList();

        await Task.WhenAll(tasks);

        return results;
    }

    public async Task<ReadOnlyMemory<byte>> RequestSpeechSample(string text,
        TtsRequestOptions ttsRequest,
        CancellationToken cancellationToken = default)
    {
        AudioClient ??= GetClient(ttsRequest.Model!);

        return await GenerateSpeech(text, ttsRequest.Voice, ttsRequest.Speed, ttsRequest.ResponseFormat, cancellationToken);
    }

    public Task<List<Voice>?> GetVoices()
    {
        var voices = new List<Voice>
        {
            new() { Name = "Alloy",   ProviderVoiceId = "alloy" },
            new() { Name = "Ash",     ProviderVoiceId = "ash" },
            new() { Name = "Ballad",  ProviderVoiceId = "ballad" },
            new() { Name = "Coral",   ProviderVoiceId = "coral" },
            new() { Name = "Echo",    ProviderVoiceId = "echo" },
            new() { Name = "Fable",   ProviderVoiceId = "fable" },
            new() { Name = "Onyx",    ProviderVoiceId = "onyx" },
            new() { Name = "Nova",    ProviderVoiceId = "nova" },
            new() { Name = "Sage",    ProviderVoiceId = "sage" },
            new() { Name = "Shimmer", ProviderVoiceId = "shimmer" },
            new() { Name = "Verse",   ProviderVoiceId = "verse" }
        };

        return Task.FromResult<List<Voice>?>(voices);
    }

    private async Task<ReadOnlyMemory<byte>> GenerateSpeech(string text, string voice, double speed,
        SpeechResponseFormat generatedSpeechFormat,
        CancellationToken cancellationToken)
    {
        SpeechGenerationOptions options = new()
        {
            SpeedRatio = Convert.ToSingle(speed),
            ResponseFormat = generatedSpeechFormat.ToString()
        };

        var result = await AudioClient.GenerateSpeechAsync(text, voice, options, cancellationToken);

        return result.Value.ToMemory();
    }

    private AudioClient GetClient(string model) => _client.GetAudioClient(model);
}
