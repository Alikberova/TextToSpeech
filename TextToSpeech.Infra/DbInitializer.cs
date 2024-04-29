using Microsoft.EntityFrameworkCore;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Infra;

public sealed class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _dbContext;

    public DbInitializer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Initialize()
    {
        _dbContext.Database.Migrate();
    }
}
