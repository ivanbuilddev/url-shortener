using UrlShortener.Models;

namespace UrlShortener.Repositories;

public interface IUrlRepository
{
    public Task<string> GetShortUrl(string originalUrl);
    public Task<string> GetOriginalUrl(string slug);
    public Task<ShortUrl?> GetUrl(string slug);
    public Task<ShortUrl> CreateShortUrl(string originalUrl);
    public Task<ShortUrl> CreateShortUrl(string originalUrl, string slug);
    public Task<ShortUrl> UpdateShortUrl(ShortUrl shortUrl);
    public Task<bool> ExistsOriginalUrl(string originalUrl);
    public Task<bool> ExistsShortUrl(string slug);
}