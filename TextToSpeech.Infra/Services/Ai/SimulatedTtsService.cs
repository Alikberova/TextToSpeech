using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Services.FileProcessing;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// Simulates TTS processing in test mode without calling external APIs.
/// </summary>
public sealed class SimulatedTtsService : INarakeetService
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

    // INarakeetService extra method
    public Task<List<VoiceResponse>?> GetAvailableVoices()
    {
        return Task.FromResult<List<VoiceResponse>?>(TestData.GetVoicesNarakeet());
    }
}
