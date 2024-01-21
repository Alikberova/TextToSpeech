using BookToAudio.Core.Entities;

namespace BookToAudio.Core.Repositories;

public interface IAudioFileRepositoryService
{
    Task AddAudioFileAsync(byte[] audioData, Guid fileId);
    Task AddAudioFileAsync(AudioFile file);
    Task DeleteAudioFileAsync(Guid id);
    Task<List<AudioFile>> GetAllAudioFilesAsync();
    Task<AudioFile?> GetAudioFileAsync(Guid id);
    Task UpdateAudioFileAsync(AudioFile audioFile);
}
