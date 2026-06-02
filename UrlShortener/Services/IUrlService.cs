using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IUrlService
{
    public Task<GetUrlResponse> GetOriginalUrl(string slug);
    public Task<GetUrlResponse> CreateShortUrl(Guid userGuid,CreateShortUrlRequest request);
    public Task UpdateCountUrl(string slug);
}