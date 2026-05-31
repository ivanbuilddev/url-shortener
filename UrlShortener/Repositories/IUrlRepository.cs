using UrlShortener.Models;

namespace UrlShortener.Repositories;

public interface IUrlRepository
{
    public Task<List<ShortUrl>?> GetUrlsByOriginalUrl(string originalUrl);
    public Task<ShortUrl?> GetUrlBySlug(string slug);
    public Task<ShortUrl?> GetUrlByOriginalUrlWithDateFilter(string originalUrl);
    public Task<ShortUrl> CreateShortUrl(string originalUrl);
    public Task<ShortUrl> CreateShortUrl(string originalUrl, string slug);
    public Task<ShortUrl> UpdateShortUrl(ShortUrl shortUrl);
    public Task<bool> ExistsOriginalUrl(string originalUrl);
    public Task<bool> ExistsOriginalUrlWithDateFilter(string originalUrl);
    public Task<bool> ExistsShortUrl(string slug);
}