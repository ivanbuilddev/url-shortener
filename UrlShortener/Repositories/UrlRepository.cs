using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly AppDbContext _dbContext;

    public UrlRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetShortUrl(string slug)
    {
        var shortUrl = await _dbContext.ShortUrls.FirstOrDefaultAsync(x => x.Slug == slug);
        if (shortUrl == null)
        {
            return string.Empty;
        }
        return shortUrl.OriginalUrl;
    }

    public async Task<string> CreateShortUrl(string originalUrl)
    {
        var shortUrl = new ShortUrl
        {
            OriginalUrl = originalUrl,
            Slug = Guid.NewGuid().ToString()
        };
        await _dbContext.ShortUrls.AddAsync(shortUrl);
        await _dbContext.SaveChangesAsync();
        return shortUrl.Slug;
    }
}