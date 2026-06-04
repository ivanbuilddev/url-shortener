namespace UrlShortener.Frontend.Services;

public interface IStorageService
{
    public Task<string?> GetItem(string key);
    public Task SetItem(string key, string value);
    public Task RemoveItem(string key);
}