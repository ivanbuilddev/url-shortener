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

    public async Task<List<ShortUrl>?> GetUrlsByUserId(Guid userId)
    {
        return await _context.ShortUrls.Include(x => x.User).Include(x => x.ClicksInfo).Where(x => x.UserId == userId && x.IsDeleted == false).ToListAsync();
    }

    public async Task<List<ShortUrl>?> GetUrlsByOriginalUrl(string originalUrl)
    {
        var url = await _context.ShortUrls.Include(x => x.User).Include(x => x.ClicksInfo).Where(x => x.OriginalUrl == originalUrl).ToListAsync();
        if (url.Count <= 0 || url == null)
        {
            return null;
        }
        return url;
    }

    public async Task<ShortUrl?> GetUrlBySlug(string slug)
    {
        var url = await _context.ShortUrls.Include(x => x.User).Include(x => x.ClicksInfo).FirstOrDefaultAsync(x => x.Slug == slug);
        if (url == null)
        {
            return null;
        }
        return url;
    }

    public async Task<ShortUrl> CreateShortUrl(Guid userGuid, CreateShortUrlRequest request)
    {
        var shortUrl = new ShortUrl
        {
            UserId = userGuid,
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

    public async Task<bool> DeleteShortUrl(int urlId)
    {
        var url = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Id == urlId);
        if (url == null) return false;
        url.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsShortUrl(string slug)
    {
        return await _context.ShortUrls.AnyAsync(x => x.Slug == slug);
    }

    public async Task<bool> CheckIfImOwner(Guid userGuid, int urlId)
    {
        var url = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Id == urlId && x.UserId == userGuid);
        if (url == null) return false;
        return true;
    }
}