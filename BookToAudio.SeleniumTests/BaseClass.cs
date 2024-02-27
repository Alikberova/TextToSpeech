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

        //Assert.That(ExtensionManager.IsPortAvailable(ConstantsTests.Localhost, ConstantsTests.ClientPort), Is.True, "Local port is not responding");
        //Assert.That(ExtensionManager.IsPortAvailable(ConstantsTests.Localhost, ConstantsTests.ServerPort), Is.True, "Local port is not responding");

        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    [TearDown]
    protected void Cleanup()
    {
        ExtensionManager.StopProcess(ServerManager._process, ClientManager._process);
        driver.Quit();
        driver.Dispose();
    }
}
