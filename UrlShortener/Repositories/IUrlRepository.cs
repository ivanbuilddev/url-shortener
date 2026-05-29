namespace UrlShortener.Repositories;

public interface IUrlRepository
{
    public Task<string> GetShortUrl(string slug);
    public Task<string> CreateShortUrl(string originalUrl);
}