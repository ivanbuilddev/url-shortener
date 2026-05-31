using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IUrlService
{
    public Task<GetUrlResponse> GetOriginalUrl(string slug);
    public Task<GetUrlResponse> CreateShortUrl(string originalUrl);
    public Task<GetUrlResponse> CreateShortUrl(string originalUrl, string aliasUrl);
    public Task UpdateCountUrl(string slug);
}