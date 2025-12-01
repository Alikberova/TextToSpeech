using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TextToSpeech.Core;
using TextToSpeech.Infra;
using Xunit.Abstractions;

namespace TextToSpeech.SeleniumTests;

public class TestBase : IDisposable
{
    private readonly string _testId = Guid.NewGuid().ToString("N");

    protected string TestDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"TtsTest_{_testId}");
    protected string DownloadDirectory =>
        Path.Combine(TestDirectory, "Downloads");

    protected ITestOutputHelper? TestOutputHelper { get; init; }
    protected IWebDriver Driver { get; private set; } = default!;
    protected WebDriverWait Wait { get; private set; } = default!;

    public TestBase(ITestOutputHelper? testOutputHelper = null)
    {
        TestOutputHelper = testOutputHelper;
        Setup();
    }

    private void Setup()
    {
        VerifyTestDirectory();

        var options = new ChromeOptions();

        if (!HostingEnvironment.IsWindows())
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--ignore-ssl-errors");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddUserProfilePreference("download.default_directory", DownloadDirectory);

        Driver = new ChromeDriver(options);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = GetWait(Driver);

        Driver.Navigate().GoToUrl($"https://localhost:{AppConstants.ClientPort}");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Driver?.Quit();
            Driver?.Dispose();
            DeleteTestDirectory();
        }
    }

    private static WebDriverWait GetWait(IWebDriver driver)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };

        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        return wait;
    }

    private void VerifyTestDirectory()
    {
        DeleteTestDirectory();

        TestOutputHelper?.WriteLine($"Creating directory {TestDirectory}...");
        Directory.CreateDirectory(TestDirectory);
        TestOutputHelper?.WriteLine("Directory created");
    }

    private void DeleteTestDirectory()
    {
        if (Directory.Exists(TestDirectory))
        {
            TestOutputHelper?.WriteLine($"Deleting directory {TestDirectory}");
            Directory.Delete(TestDirectory, true);
        }
    }
}
