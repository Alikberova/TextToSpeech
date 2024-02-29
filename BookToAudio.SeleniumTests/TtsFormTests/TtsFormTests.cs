namespace BookToAudio.SeleniumTests.TtsFormTests;

internal sealed class Tests: BaseClass
{
    [Test]
    [TestCase("http://localhost:4200/tts-form")]
    public void  TtsFormPageTests(string url)
    {
        driver.Navigate().GoToUrl(url);

        var ttsFormLogic = new TtsFormLogic(driver);
        ttsFormLogic.SelectVoice();
        ttsFormLogic.SelectTextFile();
        ttsFormLogic.SubmitFormBtn();
        ttsFormLogic.DownloadAudioFile();
        ttsFormLogic.RemoveDownloadFile();
    }
}