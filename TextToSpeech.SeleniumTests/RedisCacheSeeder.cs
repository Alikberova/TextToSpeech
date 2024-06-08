using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.SeleniumTests;

public class RedisCacheSeeder
{
    private readonly IRedisCacheProvider _redisCacheProvider;

    public RedisCacheSeeder(IRedisCacheProvider redisCacheProvider)
    {
        _redisCacheProvider = redisCacheProvider;
    }

    public async Task SeedNarakeetVoices()
    {
        if (await _redisCacheProvider.GetCachedData<List<VoiceResponse>>(CacheKeys.VoicesNarakeet) is not null)
        {
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
    }
}
