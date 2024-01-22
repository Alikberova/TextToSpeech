using OpenAI.Audio;

namespace BookToAudio.Core.Services.Interfaces;

public interface IOpenAiService
{
    Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string model = "tts-1",
        SpeechVoice voice = SpeechVoice.Alloy,
        float speed = 1);
}