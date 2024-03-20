using BookToAudio.Core.Dto;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace BookToAudio.SeleniumTests.Pages;

internal class FeedbackPage(IWebDriver driver, WebDriverWait wait) : BasePage(driver, wait)
{
    private IWebElement Name => _driver.FindElement(By.Name("name"));
    private IWebElement UserEmail => _driver.FindElement(By.Name("userEmail"));
    private IWebElement Message => _driver.FindElement(By.Name("message"));

    public void FillForm(EmailRequest request)
    {
        Name.SendKeys(request.Name);
        UserEmail.SendKeys(request.UserEmail);
        Message.SendKeys(request.Message);
    }

    public void ClickMenu()
    {
        ClickSpanByText("Feedback");
    }

    public string GetSnackBarText()
    {
        return _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("simple-snack-bar"))).Text;
    }
}
