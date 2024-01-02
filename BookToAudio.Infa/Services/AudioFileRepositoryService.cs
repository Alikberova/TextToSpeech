using BookToAudio.Core.Entities;
using BookToAudio.Core.Repositories;

namespace BookToAudio.Infra.Services;

public class AudioFileRepositoryService : IAudioFileRepositoryService
{
    private readonly IAudioFileRepository _audioFileRepository;

    public AudioFileRepositoryService(IAudioFileRepository audioFileRepository)
    {
        _audioFileRepository = audioFileRepository;
    }

    public async Task AddAudioFileAsync(byte[] audioData, Guid fileId)
    {
        var audioFile = new AudioFile
        {
            Id = fileId,
            Data = audioData,
            CreatedAt = DateTime.UtcNow,
        };

        await _audioFileRepository.AddAudioFileAsync(audioFile);
    }

    public async Task<AudioFile?> GetAudioFileAsync(Guid id)
    {
        return await _audioFileRepository.GetAudioFileAsync(id);
    }

    public async Task<List<AudioFile>> GetAllAudioFilesAsync()
    {
        return await _audioFileRepository.GetAllAudioFilesAsync();
    }

    public async Task UpdateAudioFileAsync(AudioFile audioFile)
    {
        await _audioFileRepository.UpdateAudioFileAsync(audioFile);
    }

    public async Task DeleteAudioFileAsync(Guid id)
    {
        await _audioFileRepository.DeleteAudioFileAsync(id);
    }
}
