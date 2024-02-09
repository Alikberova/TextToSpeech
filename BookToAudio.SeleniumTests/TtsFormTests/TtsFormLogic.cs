using OpenQA.Selenium;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Support.UI;

namespace BookToAudio.SeleniumTests.TtsFormTests;

internal class TtsFormLogic
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public TtsFormLogic(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }
    private IWebElement Dropdown => _driver.FindElement(By.Id("mat-select-value-1"));
    private IWebElement PlayVoice => _driver.FindElement(By.XPath("//*[@id='mat-option-0']/mat-icon"));
    private IWebElement PauseVoice => _driver.FindElement(By.Id("pause"));
    private IWebElement ChooseVoice => _driver.FindElement(By.Id("mat-option-0"));
    private IWebElement FileInput => _driver.FindElement(By.CssSelector(".file-upload input[type='file']"));
    private IWebElement SubmitBtn => _driver.FindElement(By.XPath("//span[contains(text(), 'Submit')]"));
    private IWebElement DownloadBtn => _driver.FindElement(By.Id("download"));

    public void SelectVoice()
    {
        Dropdown.Click();
        PlayVoice.Click();
        _wait.Until(_ => PauseVoice.Displayed);
        PauseVoice.Click();
        ChooseVoice.Click();
    }

    public void SelectTextFile()
    {
        string textFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "TextTestAudioBook.txt");

        FileInput.SendKeys(textFilePath);
    }

    public void SubmitFormBtn()
    {
        SubmitBtn.Click();
    }
    public void DownloadAudioFile()
    {
        Thread.Sleep(5000);
        DownloadBtn.Click();
        Thread.Sleep(3000);
    }

    public void RemoveDownloadFile()
    {
        string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
        string downloadFile = Path.Combine(downloadPath, "TextTestAudioBook.mp3");

        FileAssert.Exists(downloadFile, "File does not exist");
        File.Delete(downloadFile);
    }
}

