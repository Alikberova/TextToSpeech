using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;
using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// 50 requests/min for model tts-1
/// </summary>
public sealed class OpenAiService(OpenAIClient _openAiClient, ILogger<OpenAiService> _logger) : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 4096;
    private const int MaxParallelChunks = 20;

    private AudioClient Client { get; set; } = default!;

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

                    _logger.LogInformation("Done {done} for {id}", done, fileId);
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
        Client ??= GetClient(ttsRequest.Model!);

        return await GenerateSpeech(text, ttsRequest.Voice, ttsRequest.Speed, ttsRequest.ResponseFormat, cancellationToken);
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

        var result = await Client.GenerateSpeechAsync(text, voice, options, cancellationToken);

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

    private AudioClient GetClient(string model) => _openAiClient.GetAudioClient(model);
}
