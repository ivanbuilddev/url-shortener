namespace UrlShortener.DTOs;

public class CreateClickInfoRequest
{
    public int ShortUrlId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string BrowserVersion { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Referrer { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}