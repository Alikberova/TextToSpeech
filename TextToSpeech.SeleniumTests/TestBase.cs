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
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace TextToSpeech.SeleniumTests;

public class TestBase : IDisposable
{
    protected static readonly string TestDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "TtsTest");
    protected IWebDriver Driver { get; private set; } = default!;
    protected WebDriverWait Wait { get; private set; } = default!;
    protected IServiceProvider ServiceProvider { get; private set; } = default!;

    public TestBase()
    {
        Setup();
    }

    private void Setup()
    {
        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        try
        {
            Task.Run(SeedRedisCache).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new Exception("--------------- An error on seeding cache ---------------- ", ex);
        }

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

        Driver = new ChromeDriver(options);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        Driver.Navigate().GoToUrl($"https://localhost:{SharedConstants.ClientPort}");

        Directory.CreateDirectory(TestDirectory);
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

            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory, true);
            }
        }
    }
    private static void ConfigureServices(IServiceCollection services)
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

        services.AddLogging(b =>
        {
            b.SetMinimumLevel(LogLevel.Information);
            b.AddSimpleConsole();
        });
    }

    private async Task SeedRedisCache()
    {
        var redisCacheSeeder = ServiceProvider.GetRequiredService<RedisCacheSeeder>();
        await redisCacheSeeder.SeedNarakeetVoices();
    }
}
