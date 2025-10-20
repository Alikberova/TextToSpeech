using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Constants;
using Xunit.Abstractions;

namespace TextToSpeech.SeleniumTests;

public sealed class RedisCacheSeeder
{
    private readonly IRedisCacheProvider _redisCacheProvider;
    private readonly ITestOutputHelper _output;

    public RedisCacheSeeder(IRedisCacheProvider redisCacheProvider, ITestOutputHelper output)
    {
        _redisCacheProvider = redisCacheProvider;
        _output = output;
    }

    public async Task SeedNarakeetVoices()
    {
        _output.WriteLine("Seeding Narakeet voices into Redis cache...");

        if (await _redisCacheProvider.GetCachedData<List<VoiceResponse>>(CacheKeys.VoicesNarakeet) is not null)
        {
            _output.WriteLine("Narakeet voices already exist in Redis cache. Skipping seeding.");

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

        _output.WriteLine("Narakeet voices seeded into Redis cache successfully.");
    }
}
