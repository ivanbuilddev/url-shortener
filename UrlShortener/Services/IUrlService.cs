using UrlShortener.Models;

namespace UrlShortener.Services;

public interface IUrlService
{
    public Task<string> GetShortUrl(string originalUrl);
    public Task<string> GetOriginalUrl(string slug);
    public Task<string> CreateShortUrl(string originalUrl);
    public Task<string> CreateShortUrl(string originalUrl, string aliasUrl);
    public Task UpdateCountUrl(string slug);
}