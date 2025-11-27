using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TextToSpeech.Core.Entities;

namespace TextToSpeech.Infra;

public sealed class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AudioFile> AudioFiles { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<TtsApi> TtsApis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TtsApi>()
            .HasMany(ss => ss.AudioFiles)
            .WithOne(af => af.TtsApi)
            .HasForeignKey(af => af.TtsApiId)
            .OnDelete(DeleteBehavior.SetNull); // Prevent cascading delete
    }
}