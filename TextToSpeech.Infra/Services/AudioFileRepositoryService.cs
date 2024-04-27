using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Repositories;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Infra.Services;

public sealed class AudioFileRepositoryService : IAudioFileRepositoryService
{
    private readonly IAudioFileRepository _audioFileRepository;

    public AudioFileRepositoryService(IAudioFileRepository audioFileRepository)
    {
        _audioFileRepository = audioFileRepository;
    }

    //todo delete if not used
    public async Task AddAudioFileAsync(byte[] audioData, Guid fileId)
    {
        var audioFile = new AudioFile
        {
            Id = fileId,
            Data = audioData,
            CreatedAt = DateTime.UtcNow,
            Status = Status.Created
        };

        await _audioFileRepository.AddAudioFileAsync(audioFile);
    }

    public async Task AddAudioFileAsync(AudioFile file)
    {
        //todo fill empty props
        await _audioFileRepository.AddAudioFileAsync(file);
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
