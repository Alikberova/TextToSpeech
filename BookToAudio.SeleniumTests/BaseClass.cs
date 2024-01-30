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
    }

    [TearDown]
    protected void TearDown()
    {
        driver.Quit();
    }
}
