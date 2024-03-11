using BookToAudio.Core;
using BookToAudio.SeleniumTests.ProcessStart;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BookToAudio.SeleniumTests;

[TestFixture]
internal class BaseClass
{
    protected IWebDriver driver;
   
    [SetUp]
    protected void Setup()
    {
        ServerManager.StartServer();
        ClientManager.StartClient();

        Assert.That(ExtensionManager.IsPortAvailable(Constants.Localhost, Constants.ClientPort), Is.True, "Local port is not responding");
        Assert.That(ExtensionManager.IsPortAvailable(Constants.Localhost, Constants.ServerPort), Is.True, "Local port is not responding");

        var options = new ChromeOptions();

        if (HostingEnvironment.IsRemote())
        {
            options.AddArgument("--headless=new");
        }

        driver = new ChromeDriver(options);
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    [TearDown]
    protected void Cleanup()
    {
        ExtensionManager.StopProcess(ServerManager._process, ClientManager._process);

        if (driver is not null)
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
