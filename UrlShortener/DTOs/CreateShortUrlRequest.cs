namespace UrlShortener.DTOs;

public class CreateShortUrlRequest
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string AliasUrl {get; set;} = string.Empty;
    public int ClickLimit { get; set; } = -1;
}