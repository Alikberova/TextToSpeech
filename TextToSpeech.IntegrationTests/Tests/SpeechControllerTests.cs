using TextToSpeech.Core.Config;
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
using Moq;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.IntegrationTests.Tests;

public class SpeechControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string AudioMpeg = "audio/mpeg";
    private const string InvalidMp3Error = "Invalid MP3 file.";

    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SpeechControllerTests()
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

        await hubConnection.StartAsync();

        // Act
        var response = await _client.PostAsync("/api/speech", GetFormData(ttsApi));

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        var completedTask = await Task.WhenAny(spechStatusUpdated.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        var audioFilePath = _factory.Services.GetRequiredService<IPathService>()
            .GetFilePathInFileStorage($"{fileId}.mp3");

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