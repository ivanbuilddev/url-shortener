namespace UrlShortener.Services;

public interface IUrlService
{
    public Task<string> GetShortUrl(string slug);
    public Task<string> CreateShortUrl(string originalUrl);
}