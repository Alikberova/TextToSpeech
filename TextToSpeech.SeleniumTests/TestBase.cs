using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Reflection;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Services;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace TextToSpeech.SeleniumTests;

public class TestBase : IDisposable
{
    protected static readonly string TestDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "TtsTest");
    protected IWebDriver Driver { get; private set; } = default!;
    protected WebDriverWait Wait { get; private set; } = default!;
    protected IServiceProvider ServiceProvider { get; private set; } = default!;

    protected readonly ITestOutputHelper? _output;

    public TestBase(ITestOutputHelper? output = null)
    {
        _output = output;
        Setup();
    }

    private void Setup()
    {
        Directory.CreateDirectory(TestDirectory);

        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        SeedRedisCache();

        var service = ChromeDriverService.CreateDefaultService();
        service.LogPath = Path.Combine(TestDirectory, "chromedriver.log");
        service.EnableVerboseLogging = true;

        var options = new ChromeOptions();

        if (!HostingEnvironment.IsWindows())
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--ignore-ssl-errors");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddUserProfilePreference("download.default_directory", TestDirectory);

        Driver = new ChromeDriver(service, options);
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

            //if (Directory.Exists(TestDirectory))
            //{
            //    Directory.Delete(TestDirectory, true);
            //}
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var conn = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        services.AddSingleton<IRedisCacheProvider>(new RedisCacheProvider(conn));
        services.AddSingleton<RedisCacheSeeder>();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            if (_output != null)
            {
                builder.AddXUnit(_output); // logs to test output
            }
            else
            {
                builder.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "HH:mm:ss ";
                    options.SingleLine = true;
                });
            }
        });
    }

    private void SeedRedisCache()
    {
        // todo should be localhost:6379; add RedisPort
        var redisCacheSeeder = ServiceProvider.GetRequiredService<RedisCacheSeeder>();

        if (!redisCacheSeeder.IsRedisConnected())
        {
            throw new InvalidOperationException("Redis is not connected. Cannot seed cache.");
        }

        redisCacheSeeder.SeedNarakeetVoices().GetAwaiter().GetResult();
    }
}
