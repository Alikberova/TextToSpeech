using TextToSpeech.Core.Entities;

namespace TextToSpeech.Core.Interfaces.Repositories;

public interface ITranslationRepository
{
    Task AddTranslationAsync(Translation translation);
    Task DeleteTranslationAsync(Guid id);
    Task<List<Translation>> GetAllTranslationsAsync();
    Task<Translation?> GetTranslationAsync(Guid id);
    Task<Translation?> GetTranslationAsync(string sourceLang, string targetLang, string originalText);
    Task UpdateTranslationAsync(Translation translation);
}