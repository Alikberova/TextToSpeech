using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.EntityFrameworkCore;
using TextToSpeech.Infra;
using TextToSpeech.SeleniumTests.Pages;
using System.Text;
using System.Security.Cryptography;
using TextToSpeech.Core.Entities;

namespace TextToSpeech.SeleniumTests;

public class TestBase : IDisposable
{
    protected static readonly string DownloadDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "BtaDownloads");
    protected IWebDriver Driver { get; private set; } = default!;
    protected WebDriverWait Wait { get; private set; } = default!;
    private readonly AppDbContext _context;

    public TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new AppDbContext(options);

        SeedTestData(_context, TextToSpeechPage.VoiceToChange);
        Setup();
    }

    private void Setup()
    {
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

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        Driver.Navigate().GoToUrl($"https://localhost:{SharedConstants.ClientPort}");
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

    private static void SeedTestData(AppDbContext context, string voice)
    {
        const string langCode = "en-US";

        if (context.AudioFiles.Any(a => a.LanguageCode.StartsWith(langCode) && a.Voice == voice))
        {
            return;
        }

        const string uiDemoText = "Welcome to our voice showcase! Listen as we bring words to life, demonstrating a range of unique and dynamic vocal styles!";

        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(uiDemoText)));

        context.Add(new AudioFile()
        {
            Id = Guid.NewGuid(),
            Hash = hash,
            Speed = 1,
            Voice = voice,
            LanguageCode = langCode
        });

        context.SaveChanges();
    }
}
