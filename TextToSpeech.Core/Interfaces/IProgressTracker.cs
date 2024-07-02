namespace TextToSpeech.Core.Interfaces;

public interface IProgressTracker
{
    int GetOverallProgress(Guid fileId);
    void UpdateProgress(Guid fileId, int chunkIndex, int progress);
}