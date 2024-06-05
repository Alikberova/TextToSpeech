using Google.Cloud.Translation.V2;

namespace TextToSpeech.Infra.Services.Interfaces;

public interface ITranslationClientWrapper
{
    Task<TranslationResult> TranslateTextAsync(string text, string targetLanguage, string sourceLanguage);
}