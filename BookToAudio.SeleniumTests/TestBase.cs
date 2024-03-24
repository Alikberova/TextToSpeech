using BookToAudio.Core;
using BookToAudio.Core.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BookToAudio.SeleniumTests;

public class TestBase : IDisposable
{
    protected static readonly string DownloadDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "BtaDownloads");
    protected IWebDriver Driver { get; private set; } = default!;
    protected WebDriverWait Wait { get; private set; } = default!;

    public TestBase()
    {
        Setup();
    }

    private void Setup()
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

        options.AddUserProfilePreference("download.default_directory", DownloadDirectory);

        Driver = new ChromeDriver(options);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        Driver.Navigate().GoToUrl($"http://localhost:{SharedConstants.ClientPort}");
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
            if (Driver is not null)
            {
                Driver.Quit();
                Driver.Dispose();
            }

            if (Directory.Exists(DownloadDirectory))
            {
                Directory.Delete(DownloadDirectory, true);
            }
        }
    }
}
