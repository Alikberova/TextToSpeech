using System.Net;
using System.Text;
using System.Text.Json;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Dto.Narakeet;
using TextToSpeech.Infra.Services.FileProcessing;
using static TextToSpeech.Infra.TestData;

namespace TextToSpeech.Infra.Stubs;

public sealed class FakeNarakeetHandler : HttpMessageHandler
{
    public Uri BaseAddress { get; } = new("https://fake.narakeet.local/");
    public const int BuildTaskStatusPercentInProgress = 42;

    private readonly BuildTask _buildTask;
    private readonly BuildTaskStatus _inProgress;
    private readonly BuildTaskStatus _finished;
    private readonly byte[] _audioBytes;

    private readonly List<NarakeetVoiceResult> _voices;

    private int _statusCalls;

    public FakeNarakeetHandler()
    {
        var taskId = Guid.NewGuid().ToString("N");

        _buildTask = new BuildTask
        {
            TaskId = taskId,
            RequestId = $"req-{taskId}",
            StatusUrl = $"{BaseAddress}status/{taskId}"
        };

        _inProgress = new BuildTaskStatus
        {
            Finished = false,
            Succeeded = false,
            Percent = BuildTaskStatusPercentInProgress,
            Message = "processing"
        };

        _finished = new BuildTaskStatus
        {
            Finished = true,
            Succeeded = true,
            Percent = 100,
            Result = $"{BaseAddress}result/{taskId}"
        };

        _audioBytes = AudioFileService.GenerateSilentMp3(2);

        _voices = NarakeetVoices.All
            .Select(FromVoice)
            .ToList();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        var path = uri?.AbsolutePath ?? "";
        var url = uri?.ToString() ?? "";

        if (request.Method == HttpMethod.Get &&
            string.Equals(path.TrimEnd('/'), "/voices", StringComparison.OrdinalIgnoreCase))
        {
            await Delay.RandomShort(cancellationToken);
            return Json(HttpStatusCode.OK, _voices);
        }

        // POST → create task
        if (request.Method == HttpMethod.Post)
        {
            await Delay.RandomShort(cancellationToken);
            return Json(HttpStatusCode.OK, _buildTask);
        }

        // GET status → in progress once, then finished
        if (request.Method == HttpMethod.Get &&
            string.Equals(url, _buildTask.StatusUrl, StringComparison.Ordinal))
        {
            await Delay.RandomShort(cancellationToken);

            _statusCalls++;
            return Json(HttpStatusCode.OK,
                _statusCalls == 1 ? _inProgress : _finished);
        }

        // GET result → audio bytes
        if (request.Method == HttpMethod.Get &&
            string.Equals(url, _finished.Result, StringComparison.Ordinal))
        {
            await Delay.RandomShort(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_audioBytes)
            };
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private static NarakeetVoiceResult FromVoice(Voice voice)
    {
        return new NarakeetVoiceResult
        {
            Name = voice.ProviderVoiceId,
            Language = voice.Language?.Name ?? string.Empty,
            LanguageCode = voice.Language?.LanguageCode ?? string.Empty,
            Styles = []
        };
    }

    private static HttpResponseMessage Json<T>(HttpStatusCode status, T value) =>
        new(status)
        {
            Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json")
        };
}
