namespace TextToSpeech.Core.Interfaces.Ai;

public interface ITranslationService
{
    Task<string> Translate(string text, string sourceLanguage, string targetLanguage);
}