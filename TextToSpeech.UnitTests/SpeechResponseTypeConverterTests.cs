using System.ComponentModel;
using TextToSpeech.Core.Models;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class SpeechResponseTypeConverterTests
{
    private readonly TypeConverter _converter = TypeDescriptor.GetConverter(typeof(SpeechResponseFormat));

    [Fact]
    public void ConvertFrom_ValidString_ReturnsExpectedFormat()
    {
        var result = _converter.ConvertFrom(SpeechResponseFormat.Wav.ToString());

        Assert.Equal(SpeechResponseFormat.Wav, result);
    }

    [Fact]
    public void ConvertFrom_InvalidString_ThrowsNotSupportedException()
    {
        var request = SpeechResponseFormat.Wav.ToString().ToUpperInvariant();

        var ex = Assert.Throws<NotSupportedException>(() => _converter.ConvertFrom(request));
        var expected = SpeechResponseFormat.UnsupportedFormatError(request);
        Assert.Equal(expected, ex.Message);
    }
}
