using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.DependencyInjection;
using TextToSpeech.Infra.Services.Interfaces;
using TextToSpeech.Infra.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

        SeedRedisCache().GetAwaiter().GetResult();

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

        services.AddSingleton<IRedisCacheProvider>(new RedisCacheProvider(configuration.GetConnectionString("Redis")!));
        services.AddSingleton<RedisCacheSeeder>();
    }

    private async Task SeedRedisCache()
    {
        var redisCacheSeeder = ServiceProvider.GetRequiredService<RedisCacheSeeder>();
        await redisCacheSeeder.SeedNarakeetVoices().ConfigureAwait(false);
    }
}
