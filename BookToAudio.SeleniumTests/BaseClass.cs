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

        Assert.That(ExtensionManager.IsPortAvailable(ConstantsTests.Localhost, ConstantsTests.PortClient), Is.True, "Local port is not responding");
        Assert.That(ExtensionManager.IsPortAvailable(ConstantsTests.Localhost, ConstantsTests.PortServer), Is.True, "Local port is not responding");


        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    [TearDown]
    protected void OneTimeSetUp()
    {
        ExtensionManager.StopProcess(ServerManager.process, ClientManager.process);
        driver.Quit();
        driver.Dispose();
    }
}
