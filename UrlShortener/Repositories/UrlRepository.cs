using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.DTOs;

namespace UrlShortener.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly AppDbContext _context;

    public UrlRepository(AppDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<List<ShortUrl>?> GetUrlsByOriginalUrl(string originalUrl)
    {
        var url = await _context.ShortUrls.Where(x => x.OriginalUrl == originalUrl).ToListAsync();
        if (url.Count <= 0 || url == null)
        {
            return null;
        }
        return url;
    }

    public async Task<ShortUrl?> GetUrlBySlug(string slug)
    {
        var url = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Slug == slug);
        if (url == null)
        {
            return null;
        }
        return url;
    }

    public async Task<ShortUrl> CreateShortUrl(CreateShortUrlRequest request)
    {
        var shortUrl = new ShortUrl
        {
            OriginalUrl = request.OriginalUrl,
            ClickLimit = request.ClickLimit,
            Slug = request.AliasUrl
        };
        await _context.ShortUrls.AddAsync(shortUrl);
        await _context.SaveChangesAsync();
        return shortUrl;
    }

    public async Task<ShortUrl> UpdateShortUrl(ShortUrl shortUrl)
    {
        _context.ShortUrls.Update(shortUrl);
        await _context.SaveChangesAsync();
        return shortUrl;
    }

    public async Task<bool> ExistsShortUrl(string slug)
    {
        return await _context.ShortUrls.AnyAsync(x => x.Slug == slug);
    }

}