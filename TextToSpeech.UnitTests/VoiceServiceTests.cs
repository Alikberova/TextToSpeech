using Moq;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Infra.Services;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class VoiceServiceTests
{
    [Fact]
    public async Task GetVoices_ReturnsCachedVoices_WhenCacheIsWarm()
    {
        // Arrange
        var cached = new List<Voice> { new() { Name = "cached", ProviderVoiceId = "cached-id" } };
        var provider = "narakeet";
        var cacheKey = CacheKeys.Voices(provider);

        var cache = new Mock<IRedisCacheProvider>();
        cache.Setup(c => c.Get<List<Voice>>(cacheKey)).ReturnsAsync(cached);

        var factory = new Mock<ITtsServiceFactory>(MockBehavior.Strict);

        var service = new VoiceService(factory.Object, cache.Object);

        // Act
        var result = await service.GetVoices(provider);

        // Assert
        Assert.Same(cached, result);
        factory.Verify(f => f.Get(It.IsAny<string>()), Times.Never);
        cache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<List<Voice>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task GetVoices_FetchesFromProviderAndCaches_WhenCacheIsEmpty()
    {
        // Arrange
        var providerVoices = new List<Voice> { new() { Name = "fresh", ProviderVoiceId = "fresh-id" } };
        var provider = "narakeet";
        var cacheKey = CacheKeys.Voices(provider);

        var cache = new Mock<IRedisCacheProvider>();
        cache.Setup(c => c.Get<List<Voice>>(cacheKey)).ReturnsAsync((List<Voice>?)null);

        var ttsService = new Mock<ITtsService>();
        ttsService.Setup(s => s.GetVoices()).ReturnsAsync(providerVoices);

        var factory = new Mock<ITtsServiceFactory>();
        factory.Setup(f => f.Get(provider)).Returns(ttsService.Object);

        var service = new VoiceService(factory.Object, cache.Object);

        // Act
        var result = await service.GetVoices(provider);

        // Assert
        Assert.Same(providerVoices, result);
        factory.Verify(f => f.Get(provider), Times.Once);
        ttsService.Verify(s => s.GetVoices(), Times.Once);
        cache.Verify(c => c.Set(cacheKey, providerVoices),
            Times.Once);
    }
}
