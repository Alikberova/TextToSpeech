using BookToAudio.Core.Dto;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace BookToAudio.SeleniumTests.FeedbackTests;

internal class Feedback
{
    private const string SuccessMessage = "Your feedback was sent. Thanks!";

    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public Feedback(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;
    }

    private IWebElement MainMenuButton => _driver.FindElement(By.XPath("//span[contains(text(), 'Feedback')]"));
    private IWebElement Name => _driver.FindElement(By.Name("name"));
    private IWebElement UserEmail => _driver.FindElement(By.Name("userEmail"));
    private IWebElement Message => _driver.FindElement(By.Name("message"));
    private IWebElement SubmitBtn => _driver.FindElement(By.XPath("//span[contains(text(), 'Submit')]"));

    public void Send()
    {
        var request = new EmailRequest()
        {
            Name = "Jane",
            UserEmail = "jane@qwerty.com",
            Message = "Lorem ipsum"
        };

        MainMenuButton.Click();
        Name.SendKeys(request.Name);
        UserEmail.SendKeys(request.UserEmail);
        Message.SendKeys(request.Message);
        SubmitBtn.Click();
        var snackbar = _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("simple-snack-bar")));

        Assert.That(snackbar.Text, Does.Contain(SuccessMessage));
    }
}
