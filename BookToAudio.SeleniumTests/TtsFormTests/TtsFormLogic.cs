using OpenQA.Selenium;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace BookToAudio.SeleniumTests.TtsFormTests;

internal sealed class TtsFormLogic
{
    private const string PauseButtonId = "pause";
    private const string DownloadButtonId = "download";
    private const string TargetVoice = "Fable";

    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public TtsFormLogic(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;
    }

    private IWebElement MainMenuButton => _driver.FindElement(By.XPath("//span[contains(text(), 'Generate Speech')]"));
    private IWebElement Dropdown => _driver.FindElement(By.Name("voice"));
    private IWebElement PlayVoice => _driver.FindElement(By.XPath("//mat-icon[contains(text(), 'play_circle')]"));
    private IWebElement PauseVoice => _wait.Until(ExpectedConditions.ElementIsVisible(By.Id(PauseButtonId)));
    private IWebElement ChooseVoice => _driver.FindElement(By.XPath($"//mat-option/span[contains(text(), '{TargetVoice}')]"));
    private IWebElement FileInput => _driver.FindElement(By.CssSelector(".file-upload input[type='file']"));
    private IWebElement SubmitBtn => _driver.FindElement(By.XPath("//span[contains(text(), 'Submit')]"));
    private IWebElement DownloadBtn => _driver.FindElement(By.Id(DownloadButtonId));

    public void SelectVoice()
    {
        Dropdown.Click();
        PlayVoice.Click();
        PauseVoice.Click();
        ChooseVoice.Click();
    }

    public void ClickMainMenuButton()
    {
        MainMenuButton.Click();
    }

    public void UploadFile()
    {
        string textFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Sample.txt");

        FileInput.SendKeys(textFilePath);
    }

    public void Submit()
    {
        SubmitBtn.Click();
    }

    public void DownloadAudio()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(DownloadButtonId)));
        Thread.Sleep(500);
        DownloadBtn.Click();
        Thread.Sleep(500);
    }

    public static void RemoveDownloadedFile()
    {
        string downloadFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads", "Sample.mp3");

        FileAssert.Exists(downloadFile, "File does not exist");
        File.Delete(downloadFile);
    }
}

