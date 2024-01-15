using OpenQA.Selenium;

namespace BookToAudio.Selenium.Tests.HomePageTests;

internal class HomePageLogic
{
  private  IWebDriver driver;
    public HomePageLogic(IWebDriver driver)
    {
        this.driver = driver;
    }

   private IWebElement Dropdown => driver.FindElement(By.Id("mat-select-value-1"));
   private IWebElement PlayVoice => driver.FindElement(By.XPath("//*[@id=\"mat-option-0\"]/mat-icon"));
   private IWebElement SelectVoice => driver.FindElement(By.Id("mat-option-0"));
   private IWebElement FileInput => driver.FindElement(By.CssSelector(".file-upload input[type='file']"));
   private IWebElement SubmitBtn => driver.FindElement(By.XPath("//span[contains(text(), \"Submit\")]"));

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
