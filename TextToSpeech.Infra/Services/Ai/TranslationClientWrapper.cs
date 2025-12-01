using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using TextToSpeech.Infra.Config;
using TextToSpeech.Infra.Interfaces;

namespace TextToSpeech.Infra.Services.Ai;

public class TranslationClientWrapper : ITranslationClientWrapper
{
    private readonly TranslationClient _client;

    public TranslationClientWrapper(IConfiguration configuration)
    {
        var apiKey = configuration[ConfigConstants.GoogleCloudApiKey] ??
            throw new ArgumentException("Google Api Key cannot be null");

        _client = TranslationClient.CreateFromApiKey(apiKey);
    }

    public async Task<TranslationResult> TranslateTextAsync(string text, string targetLanguage, string sourceLanguage)
    {
        return await _client.TranslateTextAsync(text, targetLanguage, sourceLanguage);
    }
}
