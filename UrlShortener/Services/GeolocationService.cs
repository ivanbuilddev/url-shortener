namespace UrlShortener.Services;

public class GeolocationService : IGeolocationService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://ipapi.co/";
    private readonly ILogger<GeolocationService> _logger;

    public GeolocationService(HttpClient httpClient, ILogger<GeolocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetCountryCode(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return string.Empty;

        var requestUrl = $"{BaseUrl}{ipAddress}/json/";
        _logger.LogInformation("GetCountryCode: requesting {requestUrl}", requestUrl);
        try
        {
            var response = await _httpClient.GetFromJsonAsync<string>(requestUrl);
            if(response == null) return string.Empty;
            _logger.LogInformation("GetCountryCode: response {response}", response);
            return response.ToUpperInvariant();
        }
        catch
        {
            _logger.LogInformation("GetCountryCode: error");
            return string.Empty;
        }
    }
}