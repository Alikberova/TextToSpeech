using OpenAI;
using OpenAI.Audio;

namespace BookToAudio.Core.Services.Interfaces.Ai;

public class OpenAiService : IOpenAiService
{
    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(SpeechRequest originalRequest, List<string> textChunks)
    {
        OpenAIClient client = new(OpenAIAuthentication.LoadFromEnv());

        // Check if there's only one chunk, then just process it
        if (textChunks.Count == 1)
        {
            return [await client.AudioEndpoint.CreateSpeechAsync(originalRequest)];
        }

        // Create a task for each text chunk
        var tasks = textChunks.Select(chunk =>
        {
            var request = new SpeechRequest(chunk, originalRequest.Model, originalRequest.Voice, originalRequest.ResponseFormat, originalRequest.Speed);
            return client.AudioEndpoint.CreateSpeechAsync(request);
        });

        // Await all tasks to complete
        ReadOnlyMemory<byte>[] results = await Task.WhenAll(tasks);

        return results;
    }
}
