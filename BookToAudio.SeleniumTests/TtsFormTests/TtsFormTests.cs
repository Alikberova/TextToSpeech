using BookToAudio.Core.Config;

namespace BookToAudio.SeleniumTests.TtsFormTests;

internal sealed class Tests : BaseClass
{
    [Test]
    public void TtsFormPageTests()
    {
        driver.Navigate().GoToUrl($"http://localhost:{SharedConstants.ClientPort}/tts-form");

        var ttsFormLogic = new TtsFormLogic(driver);
        ttsFormLogic.SelectVoice();
        ttsFormLogic.SelectTextFile();
        ttsFormLogic.SubmitFormBtn();
        ttsFormLogic.DownloadAudioFile();
        ttsFormLogic.RemoveDownloadFile();
    }
}