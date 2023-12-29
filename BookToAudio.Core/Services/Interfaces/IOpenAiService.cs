using OpenAI.Audio;

namespace BookToAudio.Core.Services.Interfaces;

public interface IOpenAiService
{
    Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(SpeechRequest request, List<string> textChunks);
}