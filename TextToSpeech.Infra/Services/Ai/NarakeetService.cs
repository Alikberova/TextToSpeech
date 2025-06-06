﻿using TextToSpeech.Core.Dto.Narakeet;
using System.Net.Http.Json;
using TextToSpeech.Infra.Constants;
using System.Text.Json;
using System.Net;
using System.Text;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core;

namespace TextToSpeech.Infra.Services.Ai;

public sealed class NarakeetService(IRedisCacheProvider _redisCacheProvider,
    HttpClient _httpClient) : INarakeetService
{
    public int MaxLengthPerApiRequest { get; init; } = 13000; //23 kb
    private const int MaxLengthForShortContent = 565;
    private const string Mp3 = "mp3";
    
    /// <summary>
    /// Requests speech with Narakeet API
    /// </summary>
    /// <param name="voice"></param>
    /// <param name="speed">value between 0.3 and 2 or fast, normal, slow</param>
    /// <returns></returns>
    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks,
        string voice,
        Guid fileId,
        double speed,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = textChunks
            .Select(chunk => RequestLongContent(chunk, voice, speed, fileId, progress,cancellationToken))
            .ToList();

        return await Task.WhenAll(tasks);
    }

    public async Task<ReadOnlyMemory<byte>> RequestSpeechSample(string text,
        string voice,
        double speed)
    {
        return await RequestShortContent(text, voice, speed);
    }

    public async Task<List<VoiceResponse>?> GetAvailableVoices() //todo move redis from here
    {
        var cachedVoices = await _redisCacheProvider.GetCachedData<List<VoiceResponse>>(CacheKeys.VoicesNarakeet);

        if (cachedVoices is not null)
        {
            return cachedVoices;
        }

        var voices = await _httpClient.GetFromJsonAsync<List<VoiceResponse>>("voices");

        await _redisCacheProvider.SetCachedData(CacheKeys.VoicesNarakeet, voices, TimeSpan.FromDays(7));

        return voices;
    }

    private async Task<ReadOnlyMemory<byte>> RequestLongContent(string text, string voice, double speed, Guid fileId,
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        AudioTaskRequest request = new()
        {
            Format = Mp3,
            Voice = voice,
            Speed = speed,
            Text = text
        };

        var buildTask = await RequestAudioTaskAsync(request, cancellationToken);

        var taskResult = await PollUntilFinishedAsync(buildTask, fileId, progress, cancellationToken);

        if (!taskResult.Succeeded)
        {
            throw new Exception($"{nameof(NarakeetService)}: Error creating audio - {taskResult.Message}");
        }

        var response = await _httpClient.GetAsync(taskResult.Result, cancellationToken);

        response.EnsureSuccessStatusCode();

        return new ReadOnlyMemory<byte>(await response.Content.ReadAsByteArrayAsync());
    }

    private async Task<BuildTask> RequestAudioTaskAsync(AudioTaskRequest audioTaskRequest, CancellationToken cancellationToken)
    {
        StringContent requestBody = new(audioTaskRequest.Text, Encoding.UTF8, "text/plain");

        using HttpResponseMessage response = await _httpClient.PostAsync(GetEndpoint(Mp3, audioTaskRequest.Voice, audioTaskRequest.Speed),
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
            var responseContent = await response.Content.ReadAsStringAsync();
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

    private async Task<ReadOnlyMemory<byte>> RequestShortContent(string text, string voice, double speed)
    {
        string sanitizedText = System.Net.WebUtility.HtmlEncode(text);
        StringContent requestBody = new(sanitizedText, Encoding.UTF8, "text/plain");

        _httpClient.DefaultRequestHeaders.Add("accept", "application/octet-stream");

        using HttpResponseMessage response = await _httpClient.PostAsync(GetEndpoint(Mp3, voice, speed), requestBody);

        response.EnsureSuccessStatusCode();

        return new ReadOnlyMemory<byte>(await response.Content.ReadAsByteArrayAsync());
    }

    private static string GetEndpoint(string format, string voice, double speed) => $"/text-to-speech/{format}?voice={voice}&voice-speed={speed}";
}
