using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.TestingInfra.DataGenerators;
using TextToSpeech.TestingInfra.Mocks;
using TextToSpeech.TestingInfra.Utils;
using Xunit.Abstractions;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.IntegrationTests.Tests;

public class SpeechControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string AudioMpeg = "audio/mpeg";
    private const string InvalidMp3Error = "Invalid MP3 file.";

    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public SpeechControllerTests(ITestOutputHelper output)
    {
        _factory = CreateFactory();
        _client = _factory.CreateClient();
        _output = output;
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

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(AudioMpeg, response.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await response.Content.ReadAsByteArrayAsync());
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
        string? errorMessage = null;
        Guid? fileId = null;
        var progressReports = new List<int?>();

        hubConnection.On<Guid, string, int?, string?>(SharedConstants.AudioStatusUpdated, (fileIdResult, updatedStatus, progressPercentage, errorMessageResult) =>
        {
            status = updatedStatus;
            errorMessage = errorMessageResult;
            fileId = fileIdResult;
            if (progressPercentage.HasValue)
            {
                progressReports.Add(progressPercentage);
            }
            if (updatedStatus == Status.Completed.ToString())
            {
                spechStatusUpdated.SetResult(true);
            }
        });

        _output.WriteLine("Starting hub connection...");

        await hubConnection.StartAsync();

        _output.WriteLine("Hub connection started");

        // Act
        var response = await _client.PostAsync("/api/speech", GetFormData(ttsApi));

        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseString);
        }

        var completedTask = await Task.WhenAny(spechStatusUpdated.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        var audioFilePath = _factory.Services.GetRequiredService<IPathService>()
            .ResolveFilePathForStorage(fileId!.Value);

        //Assert

        Assert.True(completedTask == spechStatusUpdated.Task, "Timed out to update speech status");
        Assert.Equal(Status.Completed.ToString(), status);
        Assert.True(Guid.TryParse(responseString.Trim('"'), out var respStringFileId), "Response string file ID is not a valid guid");
        Assert.Equal(respStringFileId, fileId);
        Assert.True(Mp3FileUtilities.IsMp3Valid(audioFilePath), InvalidMp3Error);
        Assert.Null(errorMessage);
        Assert.NotEmpty(progressReports);

        // Cleanup

        await hubConnection.DisposeAsync();
        File.Delete(audioFilePath);
    }

    private static TestWebApplicationFactory<Program> CreateFactory()
    {
        var factory = new TestWebApplicationFactory<Program>();

        factory.ConfigureTestServices(services =>
        {
            services.AddScoped(_ => ITtsServiceFactoryMock.Get().Object);
            services.AddScoped(_ => new Mock<IAudioFileRepository>().Object);
            services.AddTransient(_ => new Mock<IDbInitializer>().Object);
            services.AddScoped(_ => new Mock<IRedisCacheProvider>().Object);
        });

        return factory;
    }

    private HubConnection BuildHubConnection(HttpClient client, TestWebApplicationFactory<Program> factory)
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

                    // Log the original URI and the final ws/wss URI to help debug redirects
                    _output.WriteLine($"Original context URI: {context.Uri}");

                    // Ensure the scheme is ws/wss to avoid HTTP redirects (307).
                    var uri = context.Uri;
                    if (uri.Scheme == Uri.UriSchemeHttp)
                    {
                        uri = new UriBuilder(uri) { Scheme = "ws", Port = uri.Port }.Uri;
                    }
                    else if (uri.Scheme == Uri.UriSchemeHttps)
                    {
                        uri = new UriBuilder(uri) { Scheme = "wss", Port = uri.Port }.Uri;
                    }

                    _output.WriteLine($"Connecting websocket to: {uri}");

                    var res = await wsClient.ConnectAsync(uri, cancellationToken);

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
            { new StringContent(speechRequest.Speed.ToString(CultureInfo.CurrentCulture)), nameof(speechRequest.Speed) },
            { fileContent, nameof(speechRequest.File), speechRequest.File.FileName }
        };
        return formData;
    }
}