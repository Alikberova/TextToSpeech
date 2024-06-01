using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
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

        Seed();
    }

    private void Seed()
    {
        foreach (var keyValue in SharedConstants.TtsApis)
        {
            if (!_dbContext.TtsApis.Any(s => s.Id == keyValue.Value))
            {
                var service = new TtsApi()
                {
                    Name = keyValue.Key,
                    Id = keyValue.Value,
                };

                _dbContext.TtsApis.Add(service);
            }
        }

        _dbContext.SaveChanges();
    }
}
