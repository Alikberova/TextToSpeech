using OpenQA.Selenium;
using Microsoft.Win32;
using NUnit.Framework.Legacy;



namespace BookToAudio.SeleniumTests.TtsFormTests;

internal class TtsFormLogic
{
  private readonly IWebDriver _driver;

    public TtsFormLogic(IWebDriver driver)
    {
        _driver = driver;
    }
    private IWebElement Dropdown => _driver.FindElement(By.Id("mat-select-value-1"));
    private IWebElement PlayVoice => _driver.FindElement(By.XPath("//*[@id='mat-option-0']/mat-icon"));
    private IWebElement PauseVoice => _driver.FindElement(By.Id("pause"));
    private IWebElement SelectVoice => _driver.FindElement(By.Id("mat-option-0"));
    private IWebElement FileInput => _driver.FindElement(By.CssSelector(".file-upload input[type='file']"));
    private IWebElement SubmitBtn => _driver.FindElement(By.XPath("//span[contains(text(), 'Submit')]"));
    private IWebElement DownloadBtn => _driver.FindElement(By.Id("download"));

    public void SelectedVoice()
    {
        Dropdown.Click();
        PlayVoice.Click();
        PauseVoice.Click();
        SelectVoice.Click();
    }

    public void SelectedTextFile()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string removedBinFromPath = Path.GetDirectoryName(currentDirectory);
        string removedDebugFromPath = Path.GetDirectoryName(removedBinFromPath);
        string removedNetFromPath = Path.GetDirectoryName(removedDebugFromPath);
        string textFilePath = Path.Combine(removedNetFromPath, "TextTest", "TextTestAudioBook.txt");

        FileInput.SendKeys(textFilePath);
    }

    public void ClickSendBtn()
    {
        SubmitBtn.Click();
    }
    public void ClickDownloadBtn()
    {
        Thread.Sleep(5000);
        DownloadBtn.Click();
        Thread.Sleep(3000);
    }
    
    public void DeleteDownloadFile()
    {
        string regKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders";
        string downloadPath = Registry.GetValue(regKey, "{374DE290-123F-4565-9164-39C4925E467B}", null) as string;
        string downloadFile =  Path.Combine(downloadPath, "TextTestAudioBook.mp3");

        FileAssert.Exists(downloadFile, "File does not exist");
        File.Delete(downloadFile);
        FileAssert.DoesNotExist(downloadFile, "File not removed");
    }
}

