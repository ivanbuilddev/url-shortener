namespace UrlShortener.Services;

public interface IGeolocationService
{
    public Task<string> GetCountryCode(string ipAddress);
}