using OpenQA.Selenium;

namespace BookToAudio.SeleniumTests.RegisterPageTests;

internal class RegisterLogic
{
    private readonly IWebDriver _driver;

    public RegisterLogic(IWebDriver driver)
    {
        _driver = driver;
    }

    private IWebElement Name => _driver.FindElement(By.Name("username"));
    private IWebElement Password => _driver.FindElement(By.Name("password"));
    private IWebElement SignUpBtn => _driver.FindElement(By.XPath("//span[contains(text(),\"Sign Up\")]"));

    public void RegisterUser(string name, string password) 
    {
        Name.Click();
        Name.SendKeys(name);
        Password.Click();
        Password.SendKeys(password);
        SignUpBtn.Click();
    } 
}
