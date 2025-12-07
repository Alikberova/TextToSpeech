using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Dto;

namespace TextToSpeech.Infra.Services.Ai;

/// <summary>
/// <br/> The Narakeet API allows processing documents up to 100 KB for the long content (polling) API, and 1 KB for the streaming API.
/// <br/> By default, the API allows to make 86,400 requests per day (1 per second)
/// </summary>
public sealed class NarakeetService(HttpClient _httpClient) : ITtsService
{
    public int MaxLengthPerApiRequest { get; init; } = 13000; //23 kb
    private const int MaxLengthForShortContent = 565;

    /// <summary>
    /// Requests speech with Narakeet API
    /// </summary>
    /// <param name="voice"></param>
    /// <param name="speed">value between 0.3 and 2 or fast, normal, slow</param>
    /// <returns></returns>
    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        Guid fileId,
        TtsRequestOptions ttsRequest,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = textChunks
            .Select(chunk => RequestLongContent(chunk, fileId, ttsRequest, progress, cancellationToken))
            .ToList();

        return await Task.WhenAll(tasks);
    }

    public async Task<ReadOnlyMemory<byte>> RequestSpeechSample(string text,
        TtsRequestOptions ttsRequest,
        CancellationToken cancellationToken = default)
    {
        return await RequestShortContent(text, ttsRequest);
    }

    public async Task<List<Voice>?> GetVoices()
    {
        var voices = await _httpClient.GetFromJsonAsync<List<NarakeetVoiceResult>>("voices");

        if (voices is null || voices.Count == 0)
        {
            return null;
        }

        var mapped = voices.Select(v => new Voice
        {
            Name = v.Name,
            ProviderVoiceId = v.Name,
            Language = new Language(v.Language, v.LanguageCode),
        }).ToList();

        return mapped;
    }

    private async Task<ReadOnlyMemory<byte>> RequestLongContent(string text, Guid fileId,
        TtsRequestOptions ttsRequest,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var buildTask = await RequestAudioTaskAsync(text, ttsRequest, cancellationToken);

        var taskResult = await PollUntilFinishedAsync(buildTask, fileId, progress, cancellationToken);

        if (!taskResult.Succeeded)
        {
            throw new Exception($"{nameof(NarakeetService)}: Error creating audio - {taskResult.Message}");
        }

        var response = await _httpClient.GetAsync(taskResult.Result, cancellationToken);

        response.EnsureSuccessStatusCode();

        return new ReadOnlyMemory<byte>(await response.Content.ReadAsByteArrayAsync(cancellationToken));
    }

    private async Task<BuildTask> RequestAudioTaskAsync(string text, TtsRequestOptions ttsRequest, CancellationToken cancellationToken)
    {
        StringContent requestBody = new(text, Encoding.UTF8, "text/plain");

        using HttpResponseMessage response = await _httpClient.PostAsync(
            GetEndpoint(ttsRequest.ResponseFormat.ToString(), ttsRequest.Voice, ttsRequest.Speed),
            requestBody, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<BuildTask>(responseJson);

        if (result is null || string.IsNullOrWhiteSpace(result.TaskId))
        {
            throw new Exception($"{nameof(BuildTask)} was not deserialized");
        }

        return result;
    }

    private async Task<BuildTaskStatus> PollUntilFinishedAsync(BuildTask buildTask,
        Guid fileId,
        IProgress<ProgressReport>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await _httpClient.GetAsync(buildTask.StatusUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var buildTaskStatus = JsonSerializer.Deserialize<BuildTaskStatus>(responseContent);

            if (buildTaskStatus is null ||
                (string.IsNullOrWhiteSpace(buildTaskStatus.Result) && string.IsNullOrWhiteSpace(buildTaskStatus.Message)))
            {
                throw new Exception($"{nameof(BuildTaskStatus)} was not deserialized");
            }

            if (buildTaskStatus.Finished)
            {
                return buildTaskStatus;
            }

            progressCallback?.Report(new ProgressReport { FileId = fileId, ProgressPercentage = buildTaskStatus.Percent });
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    private async Task<ReadOnlyMemory<byte>> RequestShortContent(string text, TtsRequestOptions ttsRequest)
    {
        string sanitizedText = WebUtility.HtmlEncode(text);
        StringContent requestBody = new(sanitizedText, Encoding.UTF8, "text/plain");

        _httpClient.DefaultRequestHeaders.Add("accept", "application/octet-stream");

        using HttpResponseMessage response = await _httpClient.PostAsync(
            GetEndpoint(ttsRequest.ResponseFormat.ToString(), ttsRequest.Voice, ttsRequest.Speed), requestBody);

        response.EnsureSuccessStatusCode();

        return new ReadOnlyMemory<byte>(await response.Content.ReadAsByteArrayAsync());
    }

    private static string GetEndpoint(string format, string voice, double speed) => $"/text-to-speech/{format}?voice={voice}&voice-speed={speed}";
}

sealed record BuildTask
{
    [JsonPropertyName("statusUrl")]
    public string StatusUrl { get; init; } = string.Empty;

    [JsonPropertyName("taskId")]
    public string TaskId { get; init; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; init; } = string.Empty;
}

sealed record BuildTaskStatus
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("percent")]
    public int Percent { get; init; }

    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; init; }

    [JsonPropertyName("finished")]
    public bool Finished { get; init; }

    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;
}