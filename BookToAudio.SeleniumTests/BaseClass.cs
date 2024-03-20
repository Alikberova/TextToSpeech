using BookToAudio.Core;
using BookToAudio.Core.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BookToAudio.SeleniumTests;

[TestFixture]
internal class BaseClass
{
    protected IWebDriver _driver;
    protected WebDriverWait _wait;

    [SetUp]
    protected void Setup()
    {
        var options = new ChromeOptions();

        if (!HostingEnvironment.IsWindows())
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--ignore-ssl-errors");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }

        _driver = new ChromeDriver(options);
        _driver.Manage().Window.Maximize();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        _driver.Navigate().GoToUrl($"http://localhost:{SharedConstants.ClientPort}");
    }

    [TearDown]
    protected void Cleanup()
    {
        if (_driver is not null)
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
