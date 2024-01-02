using BookToAudio.Core.Entities;

namespace BookToAudio.Core.Repositories;

public interface IAudioFileRepository
{
    Task AddAudioFileAsync(AudioFile audioFile);
    Task DeleteAudioFileAsync(Guid id);
    Task<List<AudioFile>> GetAllAudioFilesAsync();
    Task<AudioFile?> GetAudioFileAsync(Guid id);
    Task UpdateAudioFileAsync(AudioFile audioFile);
}