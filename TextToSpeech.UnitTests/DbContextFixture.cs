using TextToSpeech.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace TextToSpeech.UnitTests;

public class DbContextFixture : IDisposable
{
    public AppDbContext DbContext { get; private set; }

    public DbContextFixture()
    {
        //config is not used for now; when used, need to make fix in the test.yml
        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true)
            //todo check if can change optional: true; was added for 1st version of test.yml
            .Build();
        var connectionString = config.GetConnectionString("DefaultConnection");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            //.UseInMemoryDatabase(databaseName: "TestDatabase")
            .UseNpgsql(connectionString)
            .Options;

        DbContext = new AppDbContext(options);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
