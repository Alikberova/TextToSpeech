using Moq;
using TextToSpeech.Core.Models;
using TextToSpeech.Core.Services;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class ProgressTrackerTests
{
    [Fact]
    public void UpdateProgress_AveragesAcrossAllChunksAndReports()
    {
        var tracker = new ProgressTracker();
        var fileId = Guid.NewGuid();
        var reportedPercentages = new List<int>();
        var progress = new Mock<IProgress<ProgressReport>>();
        progress.Setup(p => p.Report(It.IsAny<ProgressReport>()))
            .Callback<ProgressReport>(report =>
            {
                Assert.Equal(fileId, report.FileId);
                reportedPercentages.Add(report.ProgressPercentage);
            });

        tracker.InitializeFile(fileId, 3);

        var first = tracker.UpdateProgress(fileId, progress.Object, 0, 100);
        var second = tracker.UpdateProgress(fileId, progress.Object, 1, 100);
        var third = tracker.UpdateProgress(fileId, progress.Object, 2, 100);

        Assert.Equal(33, first);
        Assert.Equal(66, second);
        Assert.Equal(100, third);
        Assert.Equal(new[] { 33, 66, 100 }, reportedPercentages);
        progress.Verify(p => p.Report(It.IsAny<ProgressReport>()), Times.Exactly(3));
    }

    [Fact]
    public void InitializeFile_SetsZeroProgressForAllChunks()
    {
        var tracker = new ProgressTracker();
        var fileId = Guid.NewGuid();
        var progress = Mock.Of<IProgress<ProgressReport>>();

        tracker.InitializeFile(fileId, 2);

        var overall = tracker.UpdateProgress(fileId, progress, 1, 50);

        Assert.Equal(25, overall);
    }
}
