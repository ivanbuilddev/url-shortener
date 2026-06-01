using UrlShortener.Models;
using UrlShortener.DTOs;

namespace UrlShortener.Repositories;

public interface IUrlRepository
{
    public Task<List<ShortUrl>?> GetUrlsByOriginalUrl(string originalUrl);
    public Task<ShortUrl?> GetUrlBySlug(string slug);
    public Task<ShortUrl> CreateShortUrl(CreateShortUrlRequest request);
    public Task<ShortUrl> UpdateShortUrl(ShortUrl shortUrl);
    public Task<bool> ExistsShortUrl(string slug);
}