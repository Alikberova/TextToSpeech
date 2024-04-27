using TextToSpeech.Core.Entities;

namespace TextToSpeech.Core.Repositories;

public interface IAudioFileRepository
{
    Task AddAudioFileAsync(AudioFile audioFile);
    Task DeleteAudioFileAsync(Guid id);
    Task<List<AudioFile>> GetAllAudioFilesAsync();
    Task<AudioFile?> GetAudioFileAsync(Guid id);
    Task UpdateAudioFileAsync(AudioFile audioFile);
}