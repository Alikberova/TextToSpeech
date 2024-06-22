using Moq.Protected;
using Moq;
using System.Net;
using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Infra.Services.Ai;
using Xunit;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Core.Interfaces;
using System.Net.Http.Json;

namespace TextToSpeech.UnitTests;

public sealed class NarakeetServiceTests
{
    [Fact]
    public async Task GetAvailableVoices_ReturnsVoicesFromCache_WhenCachedDataExists()
    {
        // Arrange
        var cachedVoices = new List<VoiceResponse> { new() { Name = "cachedVoice" } };
        var mockRedisCacheProvider = new Mock<IRedisCacheProvider>();
        mockRedisCacheProvider.Setup(p => p.GetCachedData<List<VoiceResponse>>(CacheKeys.VoicesNarakeet))
            .ReturnsAsync(cachedVoices);

        var service = new NarakeetService(mockRedisCacheProvider.Object, new Mock<HttpClient>().Object);

        // Act
        var result = await service.GetAvailableVoices();

        // Assert
        Assert.Equal(cachedVoices, result);
    }

    [Fact]
    public async Task GetAvailableVoices_ReturnsVoicesFromApi_WhenCacheIsEmpty()
    {
        const string httpClientVoice = "httpClientVoice";

        // Arrange
        var voicesFromHttpClient = new List<VoiceResponse>() { new() { Name = httpClientVoice } };
        var mockRedisCacheProvider = new Mock<IRedisCacheProvider>();
        mockRedisCacheProvider.Setup(provider => provider.GetCachedData<List<VoiceResponse>>(CacheKeys.VoicesNarakeet))
            .ReturnsAsync((List<VoiceResponse>?)null);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(voicesFromHttpClient)
            });

        var baseUri = new Uri("http://example.com");
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = baseUri
        };

        var service = new NarakeetService(mockRedisCacheProvider.Object, httpClient);

        // Act
        var result = await service.GetAvailableVoices();

        // Assert
        Assert.Equal(httpClientVoice, result![0].Name);
    }
}