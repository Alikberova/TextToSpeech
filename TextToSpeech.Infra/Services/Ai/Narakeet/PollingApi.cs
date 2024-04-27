using System.Text;
using System.Text.Json;

namespace TextToSpeech.Infra.Services.Ai.Narakeet;

/// <summary>
/// Generated from https://github.com/narakeet/text-to-speech-polling-api-java-example/blob/master/src/main/java/com/narakeet/PollingApi.java
/// </summary>
internal sealed class PollingApi
{
    private readonly string apiKey;
    private readonly string apiUrl;
    private readonly int pollingIntervalSeconds;

    public class AudioTaskRequest
    {
        public string Voice { get; }
        public string Text { get; }
        public string Format { get; }
        public double Speed { get; }

        public AudioTaskRequest(string format, string voice, string text, double speed)
        {
            Format = format;
            Voice = voice;
            Text = text;
            Speed = speed;
        }
    }

    public class BuildTask
    {
        public string statusUrl { get; set; }
        public string taskId { get; set; }
        public string requestId { get; set; }
    }

    public class BuildTaskStatus
    {
        public string message { get; set; }
        public int percent { get; set; }
        public bool succeeded { get; set; }
        public bool finished { get; set; }
        public string result { get; set; }
    }

    public PollingApi(string apiKey, string apiUrl, int pollingIntervalSeconds)
    {
        this.apiKey = apiKey;
        this.apiUrl = apiUrl;
        this.pollingIntervalSeconds = pollingIntervalSeconds;
    }

    public PollingApi(string apiKey) : this(apiKey, "https://api.narakeet.com", 5)
    {
    }

    public async Task<BuildTask> RequestAudioTaskAsync(AudioTaskRequest audioTaskRequest)
    {
        string url = $"{apiUrl}/text-to-speech/{audioTaskRequest.Format}?voice={audioTaskRequest.Voice}&voice-speed={audioTaskRequest.Speed}";
        using HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

        StringContent requestBody = new StringContent(audioTaskRequest.Text, Encoding.UTF8, "text/plain");

        using HttpResponseMessage response = await httpClient.PostAsync(url, requestBody);
        response.EnsureSuccessStatusCode();
        string responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BuildTask>(responseJson);
    }

    public async Task<BuildTaskStatus> PollUntilFinishedAsync(BuildTask buildTask, Action<BuildTaskStatus> progressCallback = null)
    {
        while (true)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(buildTask.statusUrl);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                BuildTaskStatus buildTaskStatus = JsonSerializer.Deserialize<BuildTaskStatus>(responseContent);
                if (buildTaskStatus.finished)
                {
                    return buildTaskStatus;
                }
                progressCallback?.Invoke(buildTaskStatus);
            }
            await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds));
        }
    }

    public async Task<string> DownloadToTempFileAsync(string url, string extension)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string tempPath = Path.GetTempPath();
            string tempFileName = Path.GetRandomFileName() + "." + extension;
            string tempFilePath = Path.Combine(tempPath, tempFileName);
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            return tempFilePath;
        }
    }
}
