namespace TextToSpeech.Core.Interfaces.Ai;

public interface ITtsService
{
    int MaxInputLength { get; init; }
    Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        double speed = 1,
        string? model = null);
}