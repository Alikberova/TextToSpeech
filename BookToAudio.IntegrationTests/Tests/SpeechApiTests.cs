using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Core.Services.Interfaces.Ai;
using BookToAudio.Infra.Services.Common;
using BookToAudio.TestingInfra.DataGenerators;
using BookToAudio.TestingInfra.Mocks;
using BookToAudio.TestingInfra.Utils;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

using static BookToAudio.Core.Enums;

namespace BookToAudio.IntegrationTests.Tests;

// todo Unit tests for Signalr if speech ready - need to mock the rest
public class SpeechApiTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string AudioMpeg = "audio/mpeg";
    private const string InvalidMp3Error = "Invalid MP3 file.";

    [Fact]
    public async Task GetVoiceSample_ReturnsMp3_RealApi()
    {
        // Arrange
        var client = CreateFactory().CreateClient();

        var httpContent = new StringContent(JsonConvert.SerializeObject(SpeechRequestGenerator.GenerateFakeSpeechRequest()),
            Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/speech/sample", httpContent);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(AudioMpeg, response.Content.Headers.ContentType?.MediaType);
        Assert.True(Mp3FileUtilities.IsMp3Valid(bytes), InvalidMp3Error);
    }

    [Fact]
    public async Task Test_CreateSpeech_ReturnsMp3()
    {
        // Arrange

        var factory = CreateFactory();

        var client = factory.CreateClient();

        var hubConnection = BuildHubConnection(client, factory);

        var spechStatusUpdated = new TaskCompletionSource<bool>();

        //todo "AudioStatusUpdated" to const
        var status = string.Empty;

        hubConnection.On<Guid, string>("AudioStatusUpdated", (fileId, updatedStatus) =>
        {
            status = updatedStatus;
            spechStatusUpdated.SetResult(true);
        });

        await hubConnection.StartAsync();

        // Act

        var response = await client.PostAsync("/api/speech", GetFormData());

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        var isAudioFileIdValid = Guid.TryParse(responseString.Trim('"'), out var audioFileId);

        var completedTask = await Task.WhenAny(spechStatusUpdated.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        var audioFilePath = factory.Services.GetRequiredService<IPathService>()
            .CreateFileStorageFilePath($"{audioFileId}.mp3");

        //Assert

        Assert.True(completedTask == spechStatusUpdated.Task, "Timed out to update speech status");
        Assert.Equal(status, Status.Completed.ToString());
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
            if (!factory.RunRealApiTests)
            {
                services.AddScoped(_ => OpenAiServiceMock.Get().Object);
            }
            else
            {
                services.AddScoped<IOpenAiService, OpenAiService>();
            }
        });

        return factory;
    }

    private static HubConnection BuildHubConnection(HttpClient client, TestWebApplicationFactory<Program> factory)
    {
        // todo audioHub to const
        return new HubConnectionBuilder()
            .WithUrl($"{client.BaseAddress}audioHub", o =>
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

    private static MultipartFormDataContent GetFormData()
    {
        var speechRequest = SpeechRequestGenerator.GenerateFakeSpeechRequest(true);

        var fileContent = new StreamContent(speechRequest.File!.OpenReadStream());
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = nameof(speechRequest.File),
            FileName = speechRequest.File.FileName
        };

        var formData = new MultipartFormDataContent
        {
            { new StringContent(speechRequest.Model!), nameof(speechRequest.Model) },
            { new StringContent(speechRequest.Voice.ToString()), nameof(speechRequest.Voice) },
            { new StringContent(speechRequest.Speed.ToString(CultureInfo.InvariantCulture)), nameof(speechRequest.Speed) },
            { fileContent, nameof(speechRequest.File), speechRequest.File.FileName }
        };
        return formData;
    }
}