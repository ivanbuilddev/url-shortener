using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Frontend.Services;

public interface IUrlService
{
    public Task<List<ShortUrl>?> GetAllUrls();
    public Task<List<UrlClickInfo>?> GetUrlStats(int urlId);
    public Task<bool> Create(CreateShortUrlRequest request);
    public Task<bool> Delete(int urlId);

}