using System.Net;

namespace UrlShortener.DTOs;

public class GetUrlResponse
{
    public HttpStatusCode HttpReturnCode { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}