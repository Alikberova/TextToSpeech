using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TextToSpeech.Core.Config;

namespace TextToSpeech.SeleniumTests.Pages;

internal sealed class TextToSpeechPage(IWebDriver driver,
    WebDriverWait wait,
    string filePathToProcessToAudio) : BasePage(driver, wait)
{
    private const string DownloadButtonId = "download";
    private const string Dropdown = "-dropdown";

    private readonly string _filePathToProcessToAudio = filePathToProcessToAudio;

    private IWebElement ApiDropdown => _driver.FindElement(By.Id($"speech-service{Dropdown}"));
    private IWebElement LangDropdown => _driver.FindElement(By.Id($"voice-language{Dropdown}"));
    private IWebElement VoiceDropdown => _driver.FindElement(By.Id($"voice{Dropdown}"));
    private IWebElement PlayButton => _driver.FindElement(By.XPath("//mat-icon[contains(text(), 'play_circle')]"));
    private IWebElement PauseButton => _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//mat-icon[contains(text(), 'pause')]")));
    private IWebElement TargetVoiceDropdownButton => GetDropdownButton(TextToSpeechFormConstants.OpeAiVoiceFable);
    private IWebElement NarraketApiDropdownButton => GetDropdownButton(SharedConstants.Narakeet);
    private IWebElement LangToChangeDropdownButton => GetDropdownButton(TextToSpeechFormConstants.GermanStandard);
    private IWebElement VoiceToChangeDropdownButton => GetDropdownButton(TextToSpeechFormConstants.NarakeetVoiceHans);
    private IWebElement FileInput => _driver.FindElement(By.Id("upload-input"));
    private IWebElement DownloadBtn => _driver.FindElement(By.Id(DownloadButtonId));

    public void SelectVoice()
    {
        VoiceDropdown.Click();
        PlayButton.Click();
        PauseButton.Click();
        TargetVoiceDropdownButton.Click();
    }

    public void ClickMenu()
    {
        ClickSpanByText("Generate Speech");
    }

    public void UploadFile()
    {
        FileInput.SendKeys(_filePathToProcessToAudio);
    }

    public void DownloadFile()
    {
        _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("simple-snack-bar")));
        _wait.Until(driver =>
        {
            var elem = ExpectedConditions.ElementToBeClickable(By.Id(DownloadButtonId)).Invoke(driver);
            return elem is not null && elem.GetAttribute("disabled") is not "true";
        });

        Thread.Sleep(750);
        DownloadBtn.Click();
        Thread.Sleep(750);
    }

    public void ChangeApiToNarakeet()
    {
        ApiDropdown.Click();
        NarraketApiDropdownButton.Click();
    }

    public string GetLanguageDropdownValue()
    {
        return LangDropdown.Text;
    }

    public void ChangeLangAndVoice()
    {
        LangDropdown.Click();
        LangToChangeDropdownButton.Click();
        VoiceDropdown.Click();
        VoiceToChangeDropdownButton.Click();
    }

    public void Cancel()
    {
        ClickSpanByText("Cancel");
    }

    public bool IsDownloadButtonEnabled()
    {
        return DownloadBtn.Enabled;
    }
}

