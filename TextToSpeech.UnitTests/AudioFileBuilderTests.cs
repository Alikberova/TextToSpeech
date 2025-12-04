using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Services;
using Xunit;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.UnitTests;

public class AudioFileBuilderTests
{
    private readonly byte[] Bytes = [1, 2, 3];
    private readonly Guid TtsApiId = SharedConstants.TtsApis.First().Value;
    private const string LangCode = "en-US";
    private const AudioType Type = AudioType.Full;
    private const string FileName = "testfile.mp3";
    private const string InputText = "This is a test.";

    private static readonly TtsRequestOptions TtsRequest = new()
    {
        Voice = "TestVoice",
        Speed = 1,
        ResponseFormat = SpeechResponseFormat.Mp3,
        Model = "test-model"
    };

    [Fact]
    public void Create_ShouldReturnValidAudioFile()
    {
        // Arrange
        var id = Guid.NewGuid();
        var hash = AudioFileBuilder.GenerateHash(InputText, LangCode, TtsRequest);

        // Act
        var audioFile = AudioFileBuilder.Create(Bytes, LangCode, Type, InputText, TtsRequest, TtsApiId,
            FileName, id);

        // Assert
        Assert.Equal(id, audioFile.Id);
        Assert.Equal(FileName, audioFile.FileName);
        Assert.Equal(Bytes, audioFile.Data);
        Assert.Equal(TtsRequest.Voice, audioFile.Voice);
        Assert.Equal(LangCode, audioFile.LanguageCode);
        Assert.Equal(TtsRequest.Speed, audioFile.Speed);
        Assert.Equal(Type, audioFile.Type);
        Assert.Equal(TtsApiId, audioFile.TtsApiId);
        Assert.NotEqual(DateTime.MinValue, audioFile.CreatedAt);
        Assert.NotEmpty(audioFile.Description);
        Assert.Equal(hash, audioFile.Hash);
    }

    [Fact]
    public void Create_AudioFilesShouldBeEqual()
    {
        // Arrange, Act
        var audioFile1 = AudioFileBuilder.Create(Bytes, LangCode, Type, InputText, TtsRequest);
        var audioFile2 = AudioFileBuilder.Create(Bytes, LangCode, Type, InputText, TtsRequest);

        // Assert
        Assert.True(audioFile1 == audioFile2);
        Assert.Equal(audioFile1, audioFile2);
    }

    [Fact]
    public void Create_AudioFilesShouldNotBeEqual()
    {
        //  Arrange
        const string change = "change";

        // Act
        var audioFile = AudioFileBuilder.Create(Bytes, LangCode, Type, InputText, TtsRequest);

        // Assert
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode, Type, InputText,
            TtsRequest with { Voice = change }));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode, Type, InputText,
            TtsRequest with { Speed = 1.1 }));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode, Type, InputText,
            TtsRequest with { ResponseFormat = SpeechResponseFormat.Wav }));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode, Type, InputText,
            TtsRequest with { Model = change }));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode, Type, InputText,
            TtsRequest with { Model = null }));

        Assert.NotEqual(audioFile, AudioFileBuilder.Create([1, 2], LangCode, Type, InputText, TtsRequest));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode + change, Type, InputText, TtsRequest));
        Assert.NotEqual(audioFile, AudioFileBuilder.Create(Bytes, LangCode, Type, InputText + change, TtsRequest));
    }
}
