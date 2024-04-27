using TextToSpeech.Core.Config;
using TextToSpeech.Infra.Services.Common;
using TextToSpeech.TestingInfra.DataGenerators;
using TextToSpeech.TestingInfra.Mocks;
using TextToSpeech.TestingInfra.Utils;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

using static TextToSpeech.Core.Enums;

namespace TextToSpeech.IntegrationTests.Tests;

// todo Unit tests for Signalr if speech ready - need to mock the rest
public class SpeechApiTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string AudioMpeg = "audio/mpeg";
    private const string InvalidMp3Error = "Invalid MP3 file.";

    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SpeechApiTests()
    {
        _factory = CreateFactory();
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData(SharedConstants.OpenAI)]
    [InlineData(SharedConstants.Narakeet)]
    public async Task GetVoiceSample_ReturnsMp3Sample(string ttsApi)
    {
        // Arrange
        var httpContent = new StringContent(JsonConvert.SerializeObject(SpeechRequestGenerator.GenerateFakeSpeechRequest(ttsApi)),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/speech/sample", httpContent);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(AudioMpeg, response.Content.Headers.ContentType?.MediaType);
        Assert.True(Mp3FileUtilities.IsMp3Valid(bytes), InvalidMp3Error);
    }

    [Theory]
    [InlineData(SharedConstants.OpenAI)]
    [InlineData(SharedConstants.Narakeet)]
    public async Task CreateSpeech_ReturnsMp3(string ttsApi)
    {
        // Arrange
        var hubConnection = BuildHubConnection(_client, _factory);

        var spechStatusUpdated = new TaskCompletionSource<bool>();

        var status = string.Empty;

        hubConnection.On<Guid, string>(SharedConstants.AudioStatusUpdated, (fileId, updatedStatus) =>
        {
            status = updatedStatus;
            spechStatusUpdated.SetResult(true);
        });

        await hubConnection.StartAsync();

        // Act
        var response = await _client.PostAsync("/api/speech", GetFormData(ttsApi));

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        var isAudioFileIdValid = Guid.TryParse(responseString.Trim('"'), out var audioFileId);

        var completedTask = await Task.WhenAny(spechStatusUpdated.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        var audioFilePath = _factory.Services.GetRequiredService<IPathService>()
            .GetFileStorageFilePath($"{audioFileId}.mp3");

        //Assert

        Assert.True(completedTask == spechStatusUpdated.Task, "Timed out to update speech status");
        Assert.Equal(Status.Completed.ToString(), status);
        Assert.True(isAudioFileIdValid, "Audio file ID is not a valid guid");
        Assert.True(Mp3FileUtilities.IsMp3Valid(audioFilePath), InvalidMp3Error);

        // Cleanup

        await hubConnection.DisposeAsync();
        File.Delete(audioFilePath);
    }

    private static TestWebApplicationFactory<Program> CreateFactory()
    {
        var factory = new TestWebApplicationFactory<Program>();

        factory.ConfigureTestServices(services =>
        {
            if (factory.RunRealApiTests)
            {
                return;
            }

            services.AddScoped(_ => ITtsServiceFactoryMock.Get().Object);
        });

        return factory;
    }

    private static HubConnection BuildHubConnection(HttpClient client, TestWebApplicationFactory<Program> factory)
    {
        return new HubConnectionBuilder()
            .WithUrl($"{client.BaseAddress!.OriginalString}{SharedConstants.AudioHubEndpoint}", o =>
            {
                o.Transports = HttpTransportType.WebSockets;
                o.SkipNegotiation = true;
                o.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                o.WebSocketFactory = async (context, cancellationToken) =>
                {
                    var wsClient = factory.Server.CreateWebSocketClient();
                    var res = await wsClient.ConnectAsync(new Uri(context.Uri.ToString()), cancellationToken);
                    return res;
                };
            }).Build();
    }

    private static MultipartFormDataContent GetFormData(string ttsApi)
    {
        var speechRequest = SpeechRequestGenerator.GenerateFakeSpeechRequest(ttsApi, true);

        var fileContent = new StreamContent(speechRequest.File!.OpenReadStream());
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = nameof(speechRequest.File),
            FileName = speechRequest.File.FileName
        };

        var formData = new MultipartFormDataContent
        {
            { new StringContent(speechRequest.TtsApi), nameof(speechRequest.TtsApi) },
            { new StringContent(speechRequest.Model!), nameof(speechRequest.Model) },
            { new StringContent(speechRequest.Voice.ToString()), nameof(speechRequest.Voice) },
            { new StringContent(speechRequest.Speed.ToString(CultureInfo.InvariantCulture)), nameof(speechRequest.Speed) },
            { fileContent, nameof(speechRequest.File), speechRequest.File.FileName }
        };
        return formData;
    }
}