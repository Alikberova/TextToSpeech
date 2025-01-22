using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;

namespace TextToSpeech.SeleniumTests.Pages;

internal class BasePage(IWebDriver driver, WebDriverWait wait)
{
    protected IWebDriver _driver = driver;
    protected WebDriverWait _wait = wait;

    public void Submit()
    {
        ClickSpanByText("Submit");
    }

    protected void ClickSpanByText(string text)
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath($"//span[contains(text(), '{text}')]"))).Click();
    }

    protected IWebElement GetDropdownButton(string buttonText)
    {
        return _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath($"//mat-option/span[contains(text(), '{buttonText}')]")));
        //return _driver.FindElement(By.XPath($"//mat-option/span[contains(text(), '{buttonText}')]"));
    }
}
