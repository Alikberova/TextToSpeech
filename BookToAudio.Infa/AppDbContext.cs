using BookToAudio.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookToAudio.Infa;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
    public AppDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("");
    }

    public DbSet<TtsConversion> TtsConversions { get; set; }
}