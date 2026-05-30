namespace UrlShortener.DTOs;

public class CreateShortUrlRequest
{
    public string OriginalUrl { get; set; } = string.Empty;
}