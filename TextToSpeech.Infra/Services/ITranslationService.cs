
namespace TextToSpeech.Infra.Services
{
    public interface ITranslationService
    {
        Task<string> Translate(string text, string sourceLanguage, string targetLanguage);
    }
}