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
        ServerManager.StartedServer();
        ClientManager.StartedClient();

        Assert.That(AdvancedManagar.PortChecker(ConstantsTests.Localhost, ConstantsTests.PortClient), Is.True, "Local port is not responding");
        Assert.That(AdvancedManagar.PortChecker(ConstantsTests.Localhost, ConstantsTests.PortServer), Is.True, "Local port is not responding");

        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    [TearDown]
    protected void OneTimeSetUp()
    {
        AdvancedManagar.ProcessIsStopped(ServerManager.process, ClientManager.process);
        driver.Quit();
        driver.Dispose();
    }
}
