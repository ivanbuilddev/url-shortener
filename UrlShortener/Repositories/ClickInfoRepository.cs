using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Repositories;

public class ClickInfoRepository : IClickInfoRepository
{
    private readonly AppDbContext _context;

    public ClickInfoRepository(AppDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<List<UrlClickInfo>?> GetClickInfoByUrl(int urlId)
    {
        return await _context.UrlClicks.Include(x => x.ShortUrl).Where(x => x.ShortUrlId == urlId).ToListAsync();    
    }

    public async Task CreateClickInfo(CreateClickInfoRequest request)
    {
        var clickInfo = new UrlClickInfo
        {
            ShortUrlId = request.ShortUrlId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Browser = request.Browser,
            BrowserVersion = request.BrowserVersion,
            OperatingSystem = request.OperatingSystem,
            DeviceType = request.DeviceType,
            Referrer = request.Referrer,
            CountryCode = request.CountryCode,
        };

        await _context.UrlClicks.AddAsync(clickInfo);
        await _context.SaveChangesAsync();
    }
}