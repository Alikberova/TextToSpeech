using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Services.FileProcessing;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// Simulates TTS processing in test mode without calling external APIs.
/// </summary>
public sealed class SimulatedTtsService : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 4096;

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        Guid fileId,
        TtsRequestOptions ttsRequest,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var total = textChunks.Count;
        var simulated = new ReadOnlyMemory<byte>[total];

        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine($"#### Handling chunk: {i + 1}");

            // Simulate some work per chunk to mimic real processing
            var delayMs = 250 + (i % 5) * 75;
            await Task.Delay(delayMs, cancellationToken);

            simulated[i] = AudioFileService.GenerateSilentMp3(1);

            var completed = i + 1;
            var percentage = (int)((double)completed / total * 100);
            progress?.Report(new ProgressReport { FileId = fileId, ProgressPercentage = percentage });
        }

        return simulated;
    }

    public async Task<ReadOnlyMemory<byte>> RequestSpeechSample(string text,
        TtsRequestOptions ttsRequest,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);
        return AudioFileService.GenerateSilentMp3(2);
    }

    public Task<List<Voice>?> GetVoices()
    {
        var voices = TestData.GetVoicesNarakeet().Select(v => new Voice
        {
            Name = v.Name,
            ProviderVoiceId = v.Name,
            Language = new Language(v.Language, v.LanguageCode),
        }).ToList();

        return Task.FromResult<List<Voice>?>(voices);
    }
}
