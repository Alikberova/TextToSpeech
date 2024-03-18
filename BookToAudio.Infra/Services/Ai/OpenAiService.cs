using BookToAudio.Core.Config;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Models;

namespace BookToAudio.Core.Services.Interfaces.Ai;

public sealed class OpenAiService : IOpenAiService
{
    private IConfiguration _configuration;

    public OpenAiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string model = "tts-1",
        SpeechVoice voice = SpeechVoice.Alloy,
        float speed = 1)
    {
        OpenAIClient client = new(_configuration[ConfigConstants.OpenAiApiKey]);

        // Check if there's only one chunk, then just process it
        if (textChunks.Count == 1)
        {
            return [await client.AudioEndpoint.CreateSpeechAsync(CreateRequest(textChunks.First(), model, voice, speed))];
        }

        // Create a task for each text chunk
        var tasks = textChunks.Select(chunk =>
        {
            var request = CreateRequest(chunk, model, voice, speed);
            return client.AudioEndpoint.CreateSpeechAsync(CreateRequest(chunk, model, voice, speed));
        });

        // Await all tasks to complete
        ReadOnlyMemory<byte>[] results = await Task.WhenAll(tasks);

        return results;
    }

    private static SpeechRequest CreateRequest(string input, string model, SpeechVoice voice, float speed)
    {
        return new SpeechRequest(input, new Model(model), voice, SpeechResponseFormat.MP3, speed);
    }
}
