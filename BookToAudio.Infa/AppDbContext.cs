using BookToAudio.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookToAudio.Infa;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}
