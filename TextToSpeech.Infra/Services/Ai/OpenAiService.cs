using TextToSpeech.Core.Config;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Models;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.Infra.Services.Ai;

public sealed class OpenAiService(IConfiguration _configuration) : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 4096;

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        CancellationToken cancellationToken,
        double speed,
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
            var textChunk = textChunks.First();

            return [await client.AudioEndpoint.CreateSpeechAsync(CreateRequest(textChunks.First(), model, voiceEnum, speed),
                cancellationToken: cancellationToken)];
        }

        // Create a task for each text chunk
        var tasks = textChunks.Select(chunk =>
            client.AudioEndpoint.CreateSpeechAsync(CreateRequest(chunk, model, voiceEnum, speed), cancellationToken: cancellationToken));

        // Await all tasks to complete
        ReadOnlyMemory<byte>[] results = await Task.WhenAll(tasks);

        return results;
    }

    private async Task<ReadOnlyMemory<byte>> Test(int count, CancellationToken cancellationToken)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(1000, cancellationToken);
            Console.WriteLine(i);
        }

        return Array.Empty<byte>();  
    }

    private static SpeechRequest CreateRequest(string input, string model, SpeechVoice voice, double speed)
    {
        return new SpeechRequest(input, new Model(model), voice, SpeechResponseFormat.MP3, Convert.ToSingle(speed));
    }
}
