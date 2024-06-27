namespace TextToSpeech.Core.Interfaces.Ai;

public interface ITtsService
{
    int MaxLengthPerApiRequest { get; init; }
    Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        CancellationToken cancellationToken,
        double speed = 1,
        string? model = null);
}