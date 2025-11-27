using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Infra.Interfaces;

namespace TextToSpeech.Infra.Services;

public sealed class TranslationService : ITranslationService
{
    private readonly ITranslationClientWrapper _client;
    private readonly ITranslationRepository _translationRepository;

    public TranslationService(ITranslationClientWrapper client,
        ITranslationRepository translationRepository)
    {
        _client = client;
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
