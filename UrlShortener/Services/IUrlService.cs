using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Services;

public interface IUrlService
{
    public Task<Result<List<ShortUrl>>> GetAllUrls(Guid userGuid);
    public Task<Result<ShortUrl>> GetOriginalUrl(string slug);
    public Task<Result<ShortUrl>> CreateShortUrl(Guid userGuid,CreateShortUrlRequest request);
    public Task UpdateCountUrl(string slug);
    public Task<bool> Delete(int urlId);
    public Task<bool> CheckIfImOwner(Guid userGuid, int urlId);
}