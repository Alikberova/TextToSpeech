using BookToAudio.Core.Entities;
using BookToAudio.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookToAudio.Infra.Repositories;

public class AudioFileRepository : IAudioFileRepository
{
    private readonly AppDbContext _context;

    public AudioFileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAudioFileAsync(AudioFile audioFile)
    {
        _context.AudioFiles.Add(audioFile);
        await _context.SaveChangesAsync();
    }

    public async Task<AudioFile?> GetAudioFileAsync(Guid id)
    {
        return await _context.AudioFiles.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<AudioFile>> GetAllAudioFilesAsync()
    {
        return await _context.AudioFiles.ToListAsync();
    }

    public async Task UpdateAudioFileAsync(AudioFile audioFile)
    {
        _context.AudioFiles.Update(audioFile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAudioFileAsync(Guid id)
    {
        var audioFile = await _context.AudioFiles.FindAsync(id);
        if (audioFile != null)
        {
            _context.AudioFiles.Remove(audioFile);
            await _context.SaveChangesAsync();
        }
    }
}
