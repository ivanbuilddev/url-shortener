using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IUrlService
{
    public Task<GetAllUrlsResponse> GetAllUrls(Guid userGuid);
    public Task<GetUrlResponse> GetOriginalUrl(string slug);
    public Task<GetUrlResponse> CreateShortUrl(Guid userGuid,CreateShortUrlRequest request);
    public Task UpdateCountUrl(string slug);
    public Task<bool> Delete(int urlId);
    public Task<bool> CheckIfImOwner(Guid userGuid, int urlId);
}