using UrlShortener.Data;
using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Repositories;

public class ClickInfoRepository : IClickInfoRepository
{
    private readonly AppDbContext _dbContext;

    public ClickInfoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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

        await _dbContext.UrlClicks.AddAsync(clickInfo);
        await _dbContext.SaveChangesAsync();
    }
}