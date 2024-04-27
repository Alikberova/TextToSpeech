using TextToSpeech.Infra.Services;
using TextToSpeech.UnitTests.TestData;
using Xunit;

namespace TextToSpeech.UnitTests;

public class TextProcessingServiceTests
{
    [Theory]
    [InlineData(TextProcessingConstants.Text1500chars, 120)]
    [InlineData(TextProcessingConstants.Text1500chars, 150)]
    internal void SplitTextIfGreaterThan_ShouldSplitTextCorrectly(string text, int maxLength)
    {
        // Arrange
        var service = new TextProcessingService();

        // Act
        List<string> chunks = service.SplitTextIfGreaterThan(text, maxLength);

        // Assert
        Assert.Contains(chunks, c => c.Contains(TextProcessingConstants.CheckThatSentenceIsNotSplitByQuestionMark_Text1500chars));

        foreach (var chunk in chunks)
        {
            Assert.True(chunk.Length <= maxLength, "Each chunk must be less than or equal to maxLength");
            Assert.True(chunk.EndsWith('.') || chunk.EndsWith('?') || chunk.EndsWith('!'),
            "The string should end with '.', '?', or '!'.");
        }
    }

    // Additional tests can be added here to cover more scenarios, edge cases, and possible errors
}
