using BookToAudio.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BookToAudio.Tests;

public class DbContextFixture : IDisposable
{
    public AppDbContext DbContext { get; private set; }

    public DbContextFixture()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false, reloadOnChange: true)
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
