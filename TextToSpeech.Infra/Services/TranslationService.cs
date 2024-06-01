using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Repositories;

namespace TextToSpeech.Infra.Services;

public sealed class TranslationService : ITranslationService
{
    private readonly TranslationClient _client;
    private readonly ITranslationRepository _translationRepository;

    public TranslationService(IConfiguration configuration,
        ITranslationRepository translationRepository)
    {
        var apiKey = configuration[ConfigConstants.GoogleCloudApiKey] ??
            throw new ArgumentException("Google Api Key cannot be null");

        _client = TranslationClient.CreateFromApiKey(apiKey);
        _translationRepository = translationRepository;
    }

    public async Task<string> Translate(string text, string sourceLanguage, string targetLanguage)
    {
        var dbTranslation = await _translationRepository.GetTranslationAsync(sourceLanguage, targetLanguage, text);

        if (dbTranslation is not null)
        {
            return dbTranslation.TranslatedText;
        }

        var apiTranslation = await _client.TranslateTextAsync(text, targetLanguage, sourceLanguage);

        var newDbTranslation = new Translation()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage,
            OriginalText = text,
            TranslatedText = apiTranslation.TranslatedText
        };

        await _translationRepository.AddTranslationAsync(newDbTranslation);

        return apiTranslation.TranslatedText;
    }
}
