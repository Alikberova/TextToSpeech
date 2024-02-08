namespace BookToAudio.SeleniumTests.TtsFormTests;

public class Tests: BaseClass
{
    [Test]
    [TestCase("http://localhost:4200/tts-form")]
    public void  HomePageTests(string url)
    {
        driver.Navigate().GoToUrl(url);

        var ttsFormLogic = new TtsFormLogic(driver);
        ttsFormLogic.SelectedVoice();
        ttsFormLogic.SelectedTextFile();
        ttsFormLogic.ClickSendBtn();
        ttsFormLogic.ClickDownloadBtn();
        ttsFormLogic.DeleteDownloadFile();
    }
}