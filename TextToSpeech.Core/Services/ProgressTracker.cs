using System.Collections.Concurrent;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Core.Services;

public sealed class ProgressTracker : IProgressTracker
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, int>> progressDictionary = new();

    public void UpdateProgress(Guid fileId, int chunkIndex, int progress)
    {
        var fileProgress = progressDictionary.GetOrAdd(fileId, _ => new ConcurrentDictionary<int, int>());
        fileProgress[chunkIndex] = progress;
    }

    public int GetOverallProgress(Guid fileId)
    {
        if (!progressDictionary.TryGetValue(fileId, out var fileProgress) || fileProgress.IsEmpty)
        {
            return 0;
        }

        int sumProgress = fileProgress.Values.Sum();
        return sumProgress / fileProgress.Count;
    }
}
