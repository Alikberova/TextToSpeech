using static TextToSpeech.Core.Enums;
using Xunit;
using TextToSpeech.Infra.Services;
using TextToSpeech.Core.Config;

namespace TextToSpeech.UnitTests;

public class AudioFileBuilderTests
{
    private readonly byte[] Bytes = [1, 2, 3];
    private readonly Guid TtsApiId = SharedConstants.TtsApis.First().Value;
    private const string Voice = "TestVoice";
    private const string LangCode = "en-US";
    private const double Speed = 1.0;
    private const AudioType Type = AudioType.Full;
    private const string FileName = "testfile.mp3";

    [Fact]
    public void Create_ShouldReturnValidAudioFile()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var audioFile = AudioFileBuilder.Create(Bytes, Voice, LangCode, Speed, Type, TtsApiId, FileName, id);

        // Assert
        Assert.Equal(id, audioFile.Id);
        Assert.Equal(FileName, audioFile.FileName);
        Assert.Equal(Bytes, audioFile.Data);
        Assert.Equal(Voice, audioFile.Voice);
        Assert.Equal(LangCode, audioFile.LanguageCode);
        Assert.Equal(Speed, audioFile.Speed);
        Assert.Equal(Type, audioFile.Type);
        Assert.Equal(TtsApiId, audioFile.TtsApiId);
        Assert.NotEqual(DateTime.MinValue, audioFile.CreatedAt);
        Assert.NotEmpty(audioFile.Description);
        Assert.NotEmpty(audioFile.Hash);
    }

    [Fact]
    public void Create_AudioFilesShouldBeEqual()
    {
        // Arrange, Act
        var audioFile1 = AudioFileBuilder.Create(Bytes, Voice, LangCode, Speed, Type);
        var audioFile2 = AudioFileBuilder.Create(Bytes, Voice, LangCode, Speed, Type);

        // Assert
        Assert.Equal(audioFile1, audioFile2);
    }

    [Fact]
    public void Create_AudioFilesShouldNotBeEqual()
    {
        //  Arrange
        const string change = "change";

        // Act
        var audioFile = AudioFileBuilder.Create(Bytes, Voice, LangCode, Speed, Type);

        // Assert
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes.Reverse().ToArray(), Voice, LangCode, Speed, Type));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, Voice + change, LangCode, Speed, Type));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, Voice, LangCode + change, Speed, Type));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, Voice, LangCode, Speed + 0.1, Type));
    }
}
