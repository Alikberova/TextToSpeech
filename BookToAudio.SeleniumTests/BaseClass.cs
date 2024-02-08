using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BookToAudio.SeleniumTests;

[TestFixture]
public class BaseClass
{
    protected IWebDriver driver;

    [SetUp]
    protected void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
    }

    [TearDown]
    protected void OneTimeSetUp()
    {
        driver.Quit();
        driver.Dispose();
    }
}
