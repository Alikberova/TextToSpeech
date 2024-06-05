using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Services.Interfaces;
using TextToSpeech.SeleniumTests.Pages;

namespace TextToSpeech.SeleniumTests;

public class RedisCacheSeeder
{
    private readonly IRedisCacheProvider _redisCacheProvider;

    public RedisCacheSeeder(IRedisCacheProvider redisCacheProvider)
    {
        _redisCacheProvider = redisCacheProvider;
    }

    public async Task SeedVoicesAsync()
    {
        var voices = new List<VoiceResponse>
        {
            new() {
                Name = "anders",
                Language = "Danish",
                LanguageCode = "da-DK",
                Styles = []
            },
            new() {
                Name = "armin",
                Language = TextToSpeechPage.LangToChange,
                LanguageCode = "de-DE",
                Styles = []
            },
            new() {
                Name = TextToSpeechPage.VoiceToChange,
                Language = TextToSpeechPage.LangToChange,
                LanguageCode = "de-DE",
                Styles = []
            },
        };

        await _redisCacheProvider.SetCachedData(CacheKeys.VoicesNarakeet, voices, TimeSpan.FromDays(7));
    }
}
