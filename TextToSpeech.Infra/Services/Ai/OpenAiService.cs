using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// 50 requests/min for model tts-1
/// </summary>
public sealed class OpenAiService(OpenAIClient _client, ILogger<OpenAiService> _logger) : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 4096;
    // Limit the number of parallel chunk requests to meet rate limits
    private const int MaxParallelChunks = 20;

    private AudioClient AudioClient { get; set; } = default!;

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        Guid fileId,
        TtsRequestOptions ttsRequest,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var totalChunks = textChunks.Count;
        var completedChunks = 0;

        var results = new ReadOnlyMemory<byte>[totalChunks];

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

                    var done = Interlocked.Increment(ref completedChunks);

                    _logger.LogInformation("Done {Done} for {Id}", done, fileId);
                    ReportProgress(fileId, progress, totalChunks, done);
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

    private static void ReportProgress(Guid fileId, IProgress<ProgressReport>? progress, int totalChunks, int completedChunks)
    {
        if (progress is null)
        {
            return;
        }

        double ratio = (double)completedChunks / totalChunks;

        var progressPercentage = (int)(ratio * 100);

        progress.Report(new ProgressReport()
        {
            FileId = fileId,
            ProgressPercentage = progressPercentage
        });
    }

    private AudioClient GetClient(string model) => _client.GetAudioClient(model);
}
