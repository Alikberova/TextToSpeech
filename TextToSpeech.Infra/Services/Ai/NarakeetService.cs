using TextToSpeech.Core.Dto.Narakeet;
using System.Net.Http.Json;
using TextToSpeech.Infra.Constants;
using System.Text.Json;
using System.Text;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.Infra.Services.Ai;

public class NarakeetService : INarakeetService
{
    public int MaxLengthPerApiRequest { get; init; } = 13000; //23 kb

    private readonly HttpClient _httpClient;
    private readonly IRedisCacheProvider _redisCacheProvider;

    public NarakeetService(IRedisCacheProvider redisCacheProvider, HttpClient httpClient)
    {
        _redisCacheProvider = redisCacheProvider;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Requests speech with Narakeet API
    /// </summary>
    /// <param name="voice"></param>
    /// <param name="speed">value between 0.3 and 2 or fast, normal, slow</param>
    /// <returns></returns>
    public async Task<ReadOnlyMemory<byte>[]> RequestSpeechChunksAsync(List<string> textChunks, string voice, double speed, string? model = null)
    {
        if (textChunks.Count == 1)
        {
            return [await RequestLongContent(textChunks.First(), voice, speed)];
        }

        var tasks = textChunks.Select(chunk => RequestLongContent(chunk, voice, speed));

        return await Task.WhenAll(tasks);
    }

    public async Task<List<VoiceResponse>?> GetAvailableVoices()
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

    private async Task<ReadOnlyMemory<byte>> RequestLongContent(string text, string voice, double speed)
    {
        AudioTaskRequest request = new()
        {
            Format = "mp3",
            Voice = voice,
            Speed = speed,
            Text = text
        };

        var buildTask = await RequestAudioTaskAsync(request);

        var taskResult = await PollUntilFinishedAsync(buildTask);

        if (!taskResult.Succeeded)
        {
            throw new Exception($"{nameof(NarakeetService)}: Error creating audio - {taskResult.Message}");
        }

        var response = await _httpClient.GetAsync(taskResult.Result);

        response.EnsureSuccessStatusCode();

        return new ReadOnlyMemory<byte>(await response.Content.ReadAsByteArrayAsync());
    }

    public async Task<BuildTask> RequestAudioTaskAsync(AudioTaskRequest audioTaskRequest)
    {
        var url = $"/text-to-speech/{audioTaskRequest.Format}?voice={audioTaskRequest.Voice}&voice-speed={audioTaskRequest.Speed}";

        StringContent requestBody = new(audioTaskRequest.Text, Encoding.UTF8, "text/plain");

        using HttpResponseMessage response = await _httpClient.PostAsync(url, requestBody);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<BuildTask>(responseJson);

        if (result is null || string.IsNullOrWhiteSpace(result.TaskId))
        {
            throw new Exception($"{nameof(BuildTask)} was not deserialized");
        }

        return result;
    }

    public async Task<BuildTaskStatus> PollUntilFinishedAsync(BuildTask buildTask, Action<BuildTaskStatus>? progressCallback = null)
    {
        while (true)
        {
            var response = await _httpClient.GetAsync(buildTask.StatusUrl);
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

            progressCallback?.Invoke(buildTaskStatus);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
