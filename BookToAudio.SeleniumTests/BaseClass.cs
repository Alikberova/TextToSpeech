using BookToAudio.SeleniumTests.ProcessStart;
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
        StartClient.StartAngularApp();
        StartServer.StartWebAPI();
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    [TearDown]
    protected void OneTimeSetUp()
    {
        StartServer.StopWebAPI();
        StartClient.StopAngularApp();
        driver.Quit();
        driver.Dispose();
    }
}
