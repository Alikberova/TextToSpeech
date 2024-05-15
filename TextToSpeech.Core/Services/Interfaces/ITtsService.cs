namespace TextToSpeech.Core.Services.Interfaces;

public interface ITtsService
{
    int MaxInputLength { get; init; }
    Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        double speed = 1,
        string? model = null);
}