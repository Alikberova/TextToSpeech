using TextToSpeech.Core.Config;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Models;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.Infra.Services.Ai;

public class OpenAiService : ITtsService
{
    public int MaxInputLength { get; init; } = 4096;

    private IConfiguration _configuration;

    public OpenAiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        double speed = 1,
        string? model = null)
    {
        model ??= "tts-1";

        var parsed = Enum.TryParse<SpeechVoice>(voice, true, out var voiceEnum);

        if (!parsed)
        {
            throw new Exception("Cannot parse voice " + voice);
        }

        OpenAIClient client = new(_configuration[ConfigConstants.OpenAiApiKey]);

        // Check if there's only one chunk, then just process it
        if (textChunks.Count == 1)
        {
            return [await client.AudioEndpoint.CreateSpeechAsync(CreateRequest(textChunks.First(), model, voiceEnum, speed))];
        }

        // Create a task for each text chunk
        var tasks = textChunks.Select(chunk =>
            client.AudioEndpoint.CreateSpeechAsync(CreateRequest(chunk, model, voiceEnum, speed)));

        // Await all tasks to complete
        ReadOnlyMemory<byte>[] results = await Task.WhenAll(tasks);

        return results;
    }

    private static SpeechRequest CreateRequest(string input, string model, SpeechVoice voice, double speed)
    {
        return new SpeechRequest(input, new Model(model), voice, SpeechResponseFormat.MP3, Convert.ToSingle(speed));
    }
}
