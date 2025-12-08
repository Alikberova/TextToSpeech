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
        if (!progressDictionary.TryGetValue(fileId, out ConcurrentDictionary<int, int>? fileProgress) || fileProgress.IsEmpty)
        {
            return 0;
        }

        // overall = (sum of all the chunk percents) / chunkCount
        // example for 3 chunks: overall = (20 + 50 + 90) / 3 = 53%

        int chunkPercents = fileProgress.Values.Sum();
        int chunkCount = fileProgress.Count;

        return chunkPercents / chunkCount;
    }
}
