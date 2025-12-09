using Moq;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Models;

namespace TextToSpeech.UnitTests;

internal static class Mocks
{
    public static IProgress<ProgressReport> ProgressCallback =>
        new Mock<IProgress<ProgressReport>>().Object;

    public static IProgressTracker ProgressTracker =>
        new Mock<IProgressTracker>().Object;
}
