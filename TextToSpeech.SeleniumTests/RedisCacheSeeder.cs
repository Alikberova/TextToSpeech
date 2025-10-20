using Microsoft.Extensions.Logging;
using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Constants;

namespace TextToSpeech.SeleniumTests;

public sealed class RedisCacheSeeder
{
    private readonly IRedisCacheProvider _redisCacheProvider;
    private readonly ILogger<RedisCacheSeeder> _logger;

    public RedisCacheSeeder(IRedisCacheProvider redisCacheProvider, ILogger<RedisCacheSeeder> logger)
    {
        _redisCacheProvider = redisCacheProvider;
        _logger = logger;
    }

    public async Task SeedNarakeetVoices()
    {
        _logger.LogInformation("Seeding Narakeet voices into Redis cache...");

        if (await _redisCacheProvider.GetCachedData<List<VoiceResponse>>(CacheKeys.VoicesNarakeet) is not null)
        {
            _logger.LogInformation("Narakeet voices already exist in Redis cache. Skipping seeding.");

            return;
        }

        var voices = new List<VoiceResponse>
        {
            new() {
                Name = "anders",
                Language = "Danish",
                LanguageCode = "da-DK",
                Styles = []
            },
            new() {
                Name = "amanda",
                Language = TextToSpeechFormConstants.English,
                LanguageCode = "en-US",
                Styles = []
            },
            new() {
                Name = "armin",
                Language = TextToSpeechFormConstants.GermanStandard,
                LanguageCode = "de-DE",
                Styles = []
            },
            new() {
                Name = TextToSpeechFormConstants.NarakeetVoiceHans,
                Language = TextToSpeechFormConstants.GermanStandard,
                LanguageCode = "de-DE",
                Styles = []
            },
        };

        await _redisCacheProvider.SetCachedData(CacheKeys.VoicesNarakeet, voices, TimeSpan.FromDays(7));

        _logger.LogInformation("Narakeet voices seeded into Redis cache successfully.");
    }
}
