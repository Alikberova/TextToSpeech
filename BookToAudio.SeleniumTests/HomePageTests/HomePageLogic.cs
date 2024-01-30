using OpenQA.Selenium;

namespace BookToAudio.SeleniumTests.HomePageTests;

internal class HomePageLogic
{
  private readonly IWebDriver _driver;

    public HomePageLogic(IWebDriver driver)
    {
        _driver = driver;
    }

    private IWebElement Dropdown => _driver.FindElement(By.Id("mat-select-value-1"));
    private IWebElement PlayVoice => _driver.FindElement(By.XPath("//*[@id=\"mat-option-0\"]/mat-icon"));
    private IWebElement SelectVoice => _driver.FindElement(By.Id("mat-option-0"));
    private IWebElement FileInput => _driver.FindElement(By.CssSelector(".file-upload input[type='file']"));
    private IWebElement SubmitBtn => _driver.FindElement(By.XPath("//span[contains(text(), \"Submit\")]"));

    public void SelectedVoice()
    {
        Dropdown.Click();
        PlayVoice.Click();
        Thread.Sleep(4000);
        SelectVoice.Click();
    }

    public void SelectedTextFile()
    {
       string textFilePath = @"C:\Users\ukrbi\source\repos\BookToAudio\BookToAudio.Selenium.Tests\TextTest\TextTestAudioBook.txt";
       FileInput.SendKeys(textFilePath);
    }

    public void ClickSendBtn()
    {
        SubmitBtn.Click();
    }
}
