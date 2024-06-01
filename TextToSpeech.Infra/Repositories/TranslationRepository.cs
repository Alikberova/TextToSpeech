using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Repositories;

namespace TextToSpeech.Infra.Repositories;

public class TranslationRepository(AppDbContext context) : ITranslationRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddTranslationAsync(Translation translation)
    {
        _context.Translations.Add(translation);
        await _context.SaveChangesAsync();
    }

    public async Task<Translation?> GetTranslationAsync(Guid id)
    {
        return await _context.Translations.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Translation?> GetTranslationAsync(string sourceLang, string targetLang, string originalText)
    {
        return await _context.Translations.FirstOrDefaultAsync(t => t.SourceLanguage == sourceLang
            && t.TargetLanguage == targetLang && t.OriginalText == originalText);
    }

    public async Task<List<Translation>> GetAllTranslationsAsync()
    {
        return await _context.Translations.ToListAsync();
    }

    public async Task UpdateTranslationAsync(Translation translation)
    {
        _context.Translations.Update(translation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTranslationAsync(Guid id)
    {
        var translation = await _context.Translations.FindAsync(id);

        if (translation != null)
        {
            _context.Translations.Remove(translation);
            await _context.SaveChangesAsync();
        }
    }
}
