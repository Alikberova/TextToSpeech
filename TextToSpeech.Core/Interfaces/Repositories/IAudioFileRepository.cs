using TextToSpeech.Core.Entities;

namespace TextToSpeech.Core.Interfaces.Repositories;

public interface IAudioFileRepository
{
    Task AddAudioFileAsync(AudioFile audioFile);
    Task DeleteAudioFileAsync(Guid id);
    Task<List<AudioFile>> GetAllAudioFilesAsync();
    Task<AudioFile?> GetAudioFileAsync(Guid id);
    Task<AudioFile?> GetAudioFileAsync(string hash);
    Task<AudioFile?> GetAudioFileAsync(string hash, string voice, string languageCode, double speed);
    Task UpdateAudioFileAsync(AudioFile audioFile);
}