using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace TextToSpeech.SeleniumTests.Pages;

internal class BasePage
{
    protected IWebDriver _driver;
    protected WebDriverWait _wait;

    public BasePage(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;
    }
    public void Submit()
    {
        ClickSpanByText("Submit");
    }

    protected void ClickSpanByText(string text)
    {
        _driver.FindElement(By.XPath($"//span[contains(text(), '{text}')]")).Click();
    }

    protected IWebElement GetDropdownButton(string buttonText)
    {
        return _driver.FindElement(By.XPath($"//mat-option/span[contains(text(), '{buttonText}')]"));
    }
}
