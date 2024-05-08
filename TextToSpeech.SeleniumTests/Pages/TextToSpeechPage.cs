using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace TextToSpeech.SeleniumTests.Pages;

internal sealed class TextToSpeechPage: BasePage
{
    private const string PauseButtonId = "pause";
    private const string DownloadButtonId = "download";
    private const string TargetVoice = "Fable";

    private readonly string _sourceFilePath;

    public TextToSpeechPage(IWebDriver driver, WebDriverWait wait, string fileName) : base(driver, wait)
    {
        _sourceFilePath = fileName;
    }

    private IWebElement VoiceDropdown => _driver.FindElement(By.Id("voice-dropdown"));
    private IWebElement PlayButton => _driver.FindElement(By.XPath("//mat-icon[contains(text(), 'play_circle')]"));
    private IWebElement PauseButton => _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//mat-icon[contains(text(), 'pause')]")));
    private IWebElement TargetVoiceButton => _driver.FindElement(By.XPath($"//mat-option/span[contains(text(), '{TargetVoice}')]"));
    private IWebElement FileInput => _driver.FindElement(By.Id("upload-input"));
    private IWebElement DownloadBtn => _driver.FindElement(By.Id(DownloadButtonId));

    public void SelectVoice()
    {
        VoiceDropdown.Click();
        PlayButton.Click();
        PauseButton.Click();
        TargetVoiceButton.Click();
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
}

