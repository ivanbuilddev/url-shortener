using Microsoft.EntityFrameworkCore;
using UrlShortener.Models;

namespace UrlShortener.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<UrlClickInfo> UrlClicks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.ShortUrls)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<UrlClickInfo>()
            .HasOne(uc => uc.ShortUrl)
            .WithMany(s => s.ClicksInfo)
            .HasForeignKey(uc => uc.ShortUrlId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}   