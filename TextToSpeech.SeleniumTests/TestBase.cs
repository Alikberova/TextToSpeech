using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using StackExchange.Redis;
using Testcontainers.Redis;
using TextToSpeech.Core;
using TextToSpeech.Infra;
using TextToSpeech.Infra.Config;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Services;
using Xunit.Abstractions;
using static TextToSpeech.Infra.TestData;

namespace TextToSpeech.SeleniumTests;

public class TestBase : IAsyncLifetime
{
    private readonly string _testId = Guid.NewGuid().ToString("N");
    private readonly RedisContainer? _cacheContainer;
    private readonly bool _hasExternalCacheContainer =
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConfigConstants.CacheConnectionEnv));

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

        if (!_hasExternalCacheContainer)
        {
            _cacheContainer = new RedisBuilder()
                .WithCleanUp(true)
                .Build();
        }
    }

    public async Task InitializeAsync()
    {
        if (!_hasExternalCacheContainer)
        {
            await _cacheContainer!.StartAsync();

            Environment.SetEnvironmentVariable(ConfigConstants.CacheConnectionEnv, _cacheContainer.GetConnectionString());
        }

        await SeedCache();

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

    public async Task DisposeAsync()
    {
        Driver?.Quit();
        Driver?.Dispose();
        DeleteTestDirectory();

        if (_cacheContainer is not null)
        {
            await _cacheContainer.DisposeAsync();
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

    private static async Task SeedCache()
    {
        var mux = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable(ConfigConstants.CacheConnectionEnv)!);

        var cacheProvider = new RedisCacheProvider(mux);

        await cacheProvider.Set(CacheKeys.Voices(Shared.OpenAI.Key),
            OpenAiVoices.All, TimeSpan.FromDays(1));

        await cacheProvider.Set(CacheKeys.Voices(Shared.Narakeet.Key),
            NarakeetVoices.All, TimeSpan.FromDays(1));
    }
}
