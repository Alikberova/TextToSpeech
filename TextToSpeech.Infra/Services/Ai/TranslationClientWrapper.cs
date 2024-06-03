using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using TextToSpeech.Core.Config;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Core.Services.Interfaces.Ai;

public class TranslationClientWrapper : ITranslationClientWrapper //todo make consistent location of services/interfaces
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
