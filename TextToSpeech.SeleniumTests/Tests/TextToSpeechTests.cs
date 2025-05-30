using System.Text;
using TextToSpeech.Core.Config;
using TextToSpeech.SeleniumTests.Pages;

namespace TextToSpeech.SeleniumTests.Tests;

public sealed class TextToSpeechTests : TestBase
{
    private const string FileName = "Sample";
    private readonly string _sourceFilePath = Path.Combine(TestDirectory, $"{FileName}.txt");
    private readonly TextToSpeechPage _page;

    public TextToSpeechTests()
    {
        _page = new TextToSpeechPage(Driver, Wait, _sourceFilePath);
    }

    [Fact]
    public void TestFileExistInDb_ShouldDownloadSpeech()
    {
        File.WriteAllText(_sourceFilePath, SharedConstants.FullAudioFileContentForTesting);

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
    public async Task TestFileDoesntExistInDb_ShouldStartProcessing()
    {
        await WriteBigFileText(_sourceFilePath, SharedConstants.FullAudioFileContentForTesting, 20000);

        _page.ClickMenu();
        _page.SelectVoice();
        _page.UploadFile();
        _page.Submit();
        var progressValue = _page.GetProgressBarValue();
        _page.Cancel();

        Assert.False(_page.IsDownloadButtonEnabled(), "Download button expected to be disabled");
        Assert.False(_page.IsSnackbarPresent(), "Snack bar expected to be not present");
        Assert.True(int.TryParse(progressValue, out int progress), $"Cannot parse progress {progressValue}");
        Assert.True(progress > 0, "Progress is negative or zero");
    }

    private static async Task WriteBigFileText(string filePath, string content, int repetitions)
    {
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < repetitions; i++)
        {
            stringBuilder.Append(content);
        }

        await File.WriteAllTextAsync(filePath, stringBuilder.ToString());
    }
}