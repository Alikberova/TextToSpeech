using TextToSpeech.SeleniumTests.Pages;

namespace TextToSpeech.SeleniumTests.Tests;

public sealed class TextToSpeechTests : TestBase
{
    private const string FileName = "Sample";

    [Fact]
    public void TestTextToSpeechForm()
    {
        var sourceFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", $"{FileName}.txt");

        var page = new TextToSpeechPage(Driver, Wait, sourceFilePath);

        page.ClickMenu();
        page.SelectVoice();
        page.UploadFile();
        page.Submit();
        page.DownloadFile();
        page.ChangeApiAndLang();

        var file = Path.Combine(DownloadDirectory, $"{FileName}.mp3");

        Assert.True(File.Exists(file), $"File {file} does not exist");
    }
}