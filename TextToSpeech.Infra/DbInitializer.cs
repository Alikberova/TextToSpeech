using Microsoft.EntityFrameworkCore;

namespace TextToSpeech.Infra;

public class DbInitializer
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
