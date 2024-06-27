using TextToSpeech.Core.Config;
using TextToSpeech.SeleniumTests.Pages;

namespace TextToSpeech.SeleniumTests.Tests;

public sealed class TextToSpeechTests : TestBase
{
    private const string FileName = "Sample";
    private readonly string _surceFilePath = Path.Combine(TestDirectory, $"{FileName}.txt");
    private readonly TextToSpeechPage _page;

    public TextToSpeechTests()
    {
        _page = new TextToSpeechPage(Driver, Wait, _surceFilePath);
    }

    [Fact]
    public void TestTextToSpeechForm_ShouldDownloadSpeech()
    {
        File.WriteAllText(_surceFilePath, SharedConstants.FullAudioFileContentForTesting);

        _page.ClickMenu();
        _page.SelectVoice();
        _page.UploadFile();
        _page.Submit();
        _page.DownloadFile();
        _page.ChangeApiToNarakeet();
        var defaultNarakeetLang = _page.GetLanguageDropdownValue();
        _page.ChangeLangAndVoice();

        var file = Path.Combine(TestDirectory, $"{FileName}.mp3");

        Assert.True(File.Exists(file), $"File {file} does not exist");
        Assert.Contains(TextToSpeechFormConstants.English, defaultNarakeetLang);
    }

    [Fact]
    public void TestTextToSpeechForm_ShouldCancelSpeechProcessing()
    {
        File.WriteAllText(_surceFilePath, SharedConstants.FullAudioFileContentForTesting + " should be canceled");

        _page.ClickMenu();
        _page.SelectVoice();
        _page.UploadFile();
        _page.Submit();
        _page.Cancel();

        Assert.False(_page.IsDownloadButtonEnabled());
    }
}