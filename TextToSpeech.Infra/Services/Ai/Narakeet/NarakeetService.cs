using TextToSpeech.Core.Config;
using TextToSpeech.Core.Services.Interfaces;
using Microsoft.Extensions.Options;
using static TextToSpeech.Infra.Services.Ai.Narakeet.PollingApi;
using TextToSpeech.Infra.Services.Interfaces;
using TextToSpeech.Core.Dto.Narakeet;
using System.Net.Http.Json;

namespace TextToSpeech.Infra.Services.Ai.Narakeet;

public class NarakeetService : ITtsService, INarakeetService
{
    public int MaxInputLength { get; init; } = 13000; //23 kb

    private readonly NarakeetConfig _narakeetConfig;
    private readonly HttpClient _httpClient;
    private readonly IRedisCacheProvider _redisCacheProvider;

    public NarakeetService(IOptions<NarakeetConfig> narakeetConfig, IRedisCacheProvider redisCacheProvider)
    {
        _narakeetConfig = narakeetConfig.Value;
        _redisCacheProvider = redisCacheProvider;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _narakeetConfig.ApiKey);
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
        var cachedVoices = await _redisCacheProvider.GetCachedData<List<VoiceResponse>>("VoicesNarakeet");

        if (cachedVoices is not null)
        {
            return cachedVoices;
        }

        var voices = await _httpClient.GetFromJsonAsync<List<VoiceResponse>>($"{_narakeetConfig.ApiUrl}/voices");

        await _redisCacheProvider.SetCachedData("VoicesNarakeet", voices, TimeSpan.FromDays(7));

        return voices;
    }

    private async Task<ReadOnlyMemory<byte>> RequestLongContent(string text, string voice, double speed)
    {
        PollingApi api = new(_narakeetConfig.ApiKey);

        AudioTaskRequest request = new("mp3", voice, text, speed);

        var buildTask = await api.RequestAudioTaskAsync(request);

        var taskResult = await api.PollUntilFinishedAsync(buildTask);

        if (!taskResult.succeeded)
        {
            throw new Exception($"{nameof(NarakeetService)}: Error creating audio - {taskResult.message}");
        }

        var response = await _httpClient.GetAsync(taskResult.result);

        response.EnsureSuccessStatusCode();

        return new ReadOnlyMemory<byte>(await response.Content.ReadAsByteArrayAsync());
    }
}
