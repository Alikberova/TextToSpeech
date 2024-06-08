using TextToSpeech.Core.Config;
using TextToSpeech.SeleniumTests.Pages;

namespace TextToSpeech.SeleniumTests.Tests;

public sealed class TextToSpeechTests : TestBase
{
    private const string FileName = "Sample";

    [Fact]
    public void TestTextToSpeechForm()
    {
        var sourceFilePath = Path.Combine(TestDirectory, $"{FileName}.txt");

        File.WriteAllText(sourceFilePath, SharedConstants.FullAudioFileContentForTesting);

        var page = new TextToSpeechPage(Driver, Wait, sourceFilePath);

        page.ClickMenu();
        page.SelectVoice();
        page.UploadFile();
        page.Submit();
        page.DownloadFile();
        page.ChangeApiToNarakeet();
        var defaultNarakeetLang = page.GetLanguageDropdownValue();
        page.ChangeLangAndVoice();

        var file = Path.Combine(TestDirectory, $"{FileName}.mp3");

        Assert.True(File.Exists(file), $"File {file} does not exist");
        Assert.Contains(TextToSpeechFormConstants.English, defaultNarakeetLang);
    }
}