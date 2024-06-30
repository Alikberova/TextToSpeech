namespace TextToSpeech.Core.Interfaces.Ai;

public interface ITtsService
{
    int MaxLengthPerApiRequest { get; init; }
    Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        Guid fileId,
        double speed = 1,
        string? model = null,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default);
}