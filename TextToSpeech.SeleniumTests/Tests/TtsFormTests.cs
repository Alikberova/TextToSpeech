using System.Diagnostics;
using System.Text;
using TextToSpeech.Infra.Constants;
using TextToSpeech.SeleniumTests.Pages;
using Xunit.Abstractions;
using static TextToSpeech.Infra.TestData;

namespace TextToSpeech.SeleniumTests.Tests;

/// <summary>
/// End-to-end tests for Home page behaviors against a running app and API.
/// Uses pre-seeded DB data (Infra.TestData) and SignalR to validate generation,
/// download, and cancellation scenarios.
/// </summary>
public sealed class TtsFormTests(ITestOutputHelper output) : TestBase(output)
{
    private TtsFormPage CreatePage() => new(Driver, Wait);

    [Fact]
    public void ShouldGenerateSpeech_AndDownload_PreSeededDbAudio()
    {
        const string fileName = "integration-seeded";
        const string sourceExt = "txt";
        const string expectedAudioExt = "mp3";

        var sourcePath = Path.Combine(TestDirectory, $"{fileName}.{sourceExt}");
        File.WriteAllText(sourcePath, TtsFullRequest);

        var page = CreatePage();

        page.SelectProvider(SharedConstants.OpenAI);
        page.SelectVoice(TextToSpeechFormConstants.OpenAiVoiceAlloy);

        page.TypeSampleText();

        page.ClickPlayButton();
        Assert.True(page.IsIconVisible("pause_circle"));

        page.ClickPlayButton();
        Assert.True(page.IsIconVisible("play_circle"));

        page.SelectProvider(SharedConstants.OpenAI);
        page.SelectVoice(TextToSpeechFormConstants.OpenAiVoiceFable);
        page.UploadFile(sourcePath);
        page.ClickSubmit();

        Assert.True(page.IsProgressPanelVisible());

        page.ClickDownload();

        var sw = Stopwatch.StartNew();

        var expectedPath = Path.Combine(DownloadDirectory, $"{fileName}.{expectedAudioExt}");

        while (sw.ElapsedMilliseconds < 3000 && !File.Exists(expectedPath))
        {
            Thread.Sleep(100);
        }

        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void ShouldCancelSpeechProcessing()
    {
        var path1 = Path.Combine(TestDirectory, "test.txt");
        File.WriteAllText(path1, "abc");
        var path2 = Path.Combine(TestDirectory, "processing.txt");
        WriteBigFileText(path2, TtsFullRequest, 500);

        var page = CreatePage();

        page.SelectProvider(SharedConstants.Narakeet);
        page.SelectLanguage(TextToSpeechFormConstants.GermanStandard);
        page.SelectVoice(TextToSpeechFormConstants.NarakeetVoiceHans);
        
        page.UploadFile(path1);
        page.RemoveUploadedFile();

        page.UploadFile(path2);
        page.ClickSubmit();

        Assert.True(page.IsProgressPanelVisible());

        // when running all selenium tests, it needs timeout to pass for some reason
        Thread.Sleep(1250);
        page.ClickCancel();

        const string cancelIcon = "cancel";
        Assert.Equal(cancelIcon, page.WaitStatusIconText(cancelIcon));
    }

    private static void WriteBigFileText(string filePath, string content, int repetitions)
    {
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < repetitions; i++)
        {
            stringBuilder.Append(content);
        }

        stringBuilder.Append(Guid.NewGuid());

        File.WriteAllText(filePath, stringBuilder.ToString());
    }
}
