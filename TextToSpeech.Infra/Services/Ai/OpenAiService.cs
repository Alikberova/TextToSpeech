using TextToSpeech.Core.Config;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Models;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core;
using Microsoft.Extensions.Logging;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// 50 requests/min for model tts1 // todo handle if more than 50
/// </summary>
public sealed class OpenAiService(IConfiguration _configuration, ILogger<OpenAiService> _logger) : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 4096;

    private readonly OpenAIClient _client = new(_configuration[ConfigConstants.OpenAiApiKey]);

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        Guid fileId,
        double speed,
        string? model = null,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        model ??= "tts-1";

        var parsed = Enum.TryParse<SpeechVoice>(voice, true, out var voiceEnum);

        if (!parsed)
        {
            throw new Exception("Cannot parse voice " + voice);
        }

        // Check if there's only one chunk, then just process it
        if (textChunks.Count == 1)
        {
            var textChunk = textChunks.First();

            return [await _client.AudioEndpoint.CreateSpeechAsync(CreateRequest(textChunks.First(), model, voiceEnum, speed),
                cancellationToken: cancellationToken)];
        }

        return await RequestSpeechCollection(textChunks, fileId, voiceEnum, speed, model, progress, cancellationToken);
    }

    private async Task<ReadOnlyMemory<byte>[]> RequestSpeechCollection(List<string> textChunks,
        Guid fileId,
        SpeechVoice voiceEnum,
        double speed,
        string model,
        IProgress<ProgressReport>? progress,
        CancellationToken cancellationToken)
    {
        var totalChunks = textChunks.Count;
        var completedChunks = 0;

        var tasks = textChunks.Select((chunk, index) =>
        {
            return Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                completedChunks++;
                ReportProgress(fileId, progress, totalChunks, completedChunks);
                return await RequestSpeechWIthRetries(fileId, completedChunks, CreateRequest(chunk, model, voiceEnum, speed));
            }, cancellationToken);
        }).ToList(); // Convert to list to materialize the tasks;

        // Await all tasks to complete
        return await Task.WhenAll(tasks);
    }

    private async Task<ReadOnlyMemory<byte>> RequestSpeechWIthRetries(Guid fileId, int completedChunks, SpeechRequest request)
    {
        int attempt = 0;
        const int maxAttempts = 3;
        ReadOnlyMemory<byte> result = default;
        while (attempt < maxAttempts)
        {
            try
            {
                if (!HostingEnvironment.IsDevelopment())
                {
                    result = await _client.AudioEndpoint.CreateSpeechAsync(request);
                }
                else
                {
                    result = await Test(request.Input);
                }
                
                break; // Successful, break out of retry loop
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("The server had an error processing your request. Sorry about that!"))
            {
                _logger.LogError(ex.Message);

                attempt++;

                if (attempt == maxAttempts)
                {
                    // Max retries reached, log or handle appropriately
                    throw new Exception($"Failed after {maxAttempts} retries. Failed chunk: {completedChunks + 1}. Audio: {fileId}. " +
                        $"Last exception: {ex.Message}", ex);
                }
            }
        }

        return result;
    }

    private static void ReportProgress(Guid fileId, IProgress<ProgressReport>? progress, int totalChunks, int completedChunks)
    {
        if (progress is null)
        {
            return;
        }

        int progressPercentage = (int)((double)completedChunks / totalChunks * 100);
        progress.Report(new ProgressReport { FileId = fileId, ProgressPercentage = progressPercentage });
    }

    private async Task<ReadOnlyMemory<byte>> Test(string chunk)
    {
        // simulate processing
        //Thread.Sleep(100);
        //Console.WriteLine(chunk[..15]);
        //Console.WriteLine("Test");
        await Task.Delay(500);
        return Array.Empty<byte>();  
    }

    private static SpeechRequest CreateRequest(string input, string model, SpeechVoice voice, double speed)
    {
        return new SpeechRequest(input, new Model(model), voice, SpeechResponseFormat.MP3, Convert.ToSingle(speed));
    }
}
