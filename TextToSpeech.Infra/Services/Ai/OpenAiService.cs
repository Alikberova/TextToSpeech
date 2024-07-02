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

    /// <summary>
    /// Client shouldn't be initialized in constructor because the key is absent in tests and this will crash
    /// </summary>
    private OpenAIClient Client { get; set; } = default!;

    private const string Model = "tts-1";

    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        Guid fileId,
        double speed,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var isTestMode = HostingEnvironment.IsTestMode();

        if (!isTestMode)
        {
            Client ??= GetClient();
        }

        var voiceEnum = GetVoiceEnum(voice);
        var totalChunks = textChunks.Count;
        var completedChunks = 0;

        var tasks = textChunks.Select((chunk, index) =>
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                completedChunks++;
                ReportProgress(fileId, progress, totalChunks, completedChunks);
                if (isTestMode)
                {
                    return Test(chunk);
                }
                return RequestSpeechWithRetries(fileId, completedChunks, CreateRequest(chunk, Model, voiceEnum, speed));
            }, cancellationToken);
        }).ToList(); // Convert to list to materialize the tasks

        // Await all tasks to complete
        return await Task.WhenAll(tasks);
    }

    public async Task<ReadOnlyMemory<byte>> RequestSpeechSample(string text,
        string voice,
        double speed)
    {
        Client ??= GetClient();
        return await Client.AudioEndpoint.CreateSpeechAsync(CreateRequest(text, Model, GetVoiceEnum(voice), speed));
    }

    private async Task<ReadOnlyMemory<byte>> RequestSpeechWithRetries(Guid fileId, int completedChunks, SpeechRequest request)
    {
        int attempt = 0;
        const int maxAttempts = 3;
        ReadOnlyMemory<byte> result = default;
        while (attempt < maxAttempts)
        {
            try
            {
                result = await Client.AudioEndpoint.CreateSpeechAsync(request);

                break; // Successful, break out of retry loop
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("The server had an error processing your request. Sorry about that!"))
            {
                _logger.LogError(ex.Message);

                attempt++;

                if (attempt == maxAttempts)
                {
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
        await Task.Delay(1000);
        return Array.Empty<byte>();  
    }

    private static SpeechRequest CreateRequest(string input, string model, SpeechVoice voice, double speed)
    {
        return new SpeechRequest(input, new Model(model), voice, SpeechResponseFormat.MP3, Convert.ToSingle(speed));
    }

    private static SpeechVoice GetVoiceEnum(string voice)
    {
        return Enum.TryParse<SpeechVoice>(voice, true, out var voiceEnum) 
            ? voiceEnum
            : throw new Exception("Cannot parse voice " + voice);
    }

    private OpenAIClient GetClient() => new(_configuration[ConfigConstants.OpenAiApiKey]);
}
