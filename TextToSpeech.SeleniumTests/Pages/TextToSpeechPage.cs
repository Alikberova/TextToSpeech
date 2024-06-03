using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TextToSpeech.Core.Config;

namespace TextToSpeech.SeleniumTests.Pages;

internal sealed class TextToSpeechPage: BasePage
{
    public const string VoiceToChange = "amy";
    private const string ApiToChange = SharedConstants.Narakeet;
    private const string LangToChange = "English (British)";

    private const string DownloadButtonId = "download";
    private const string Dropdown = "-dropdown";
    private const string VoiceToPlay = "Fable";
    
    private readonly string _sourceFilePath;

    public TextToSpeechPage(IWebDriver driver, WebDriverWait wait, string fileName) : base(driver, wait)
    {
        _sourceFilePath = fileName;
    }

    private IWebElement ApiDropdown => _driver.FindElement(By.Id($"speech-service{Dropdown}"));
    private IWebElement LangDropdown => _driver.FindElement(By.Id($"voice-language{Dropdown}"));
    private IWebElement VoiceDropdown => _driver.FindElement(By.Id($"voice{Dropdown}"));
    private IWebElement PlayButton => _driver.FindElement(By.XPath("//mat-icon[contains(text(), 'play_circle')]"));
    private IWebElement PauseButton => _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//mat-icon[contains(text(), 'pause')]")));
    private IWebElement TargetVoiceDropdownButton => GetDropdownButton(VoiceToPlay);
    private IWebElement ApiToChangeDropdownButton => GetDropdownButton(ApiToChange);
    private IWebElement LangToChangeDropdownButton => GetDropdownButton(LangToChange);
    private IWebElement VoiceToChangeDropdownButton => GetDropdownButton(VoiceToChange);
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
        FileInput.SendKeys(_sourceFilePath);
    }

    public void DownloadFile()
    {
        _wait.Until(driver =>
        {
            var elem = ExpectedConditions.ElementToBeClickable(By.Id(DownloadButtonId)).Invoke(driver);
            return elem is not null && elem.GetAttribute("disabled") is not "true";
        });

        Thread.Sleep(750);
        DownloadBtn.Click();
        _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("simple-snack-bar")));
        Thread.Sleep(500);
    }

    public void ChangeApiAndLang()
    {
        ApiDropdown.Click();
        ApiToChangeDropdownButton.Click();
        LangDropdown.Click();
        LangToChangeDropdownButton.Click();
        VoiceDropdown.Click();
        VoiceToChangeDropdownButton.Click();
    }
}

