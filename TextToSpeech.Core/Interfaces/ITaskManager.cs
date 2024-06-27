namespace TextToSpeech.Core.Interfaces;

public interface ITaskManager
{
    void AddTask(Guid fileId, CancellationTokenSource cts);
    Task<bool> TryCancelTask(Guid fileId);
}