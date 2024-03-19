namespace BookToAudio.SeleniumTests.TtsFormTests;

internal sealed class TtsFormTests : BaseClass
{
    [Test]
    public void TtsFormPageTests()
    {
        var ttsFormLogic = new TtsFormLogic(_driver, _wait);

        ttsFormLogic.ClickMainMenuButton();
        ttsFormLogic.SelectVoice();
        ttsFormLogic.UploadFile();
        ttsFormLogic.Submit();
        ttsFormLogic.DownloadAudio();
        TtsFormLogic.RemoveDownloadedFile();
    }
}