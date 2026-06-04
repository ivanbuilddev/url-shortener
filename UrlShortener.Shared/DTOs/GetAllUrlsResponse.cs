using System.Net;
using UrlShortener.Models;

namespace UrlShortener.DTOs;

public class GetAllUrlsResponse
{
    public HttpStatusCode HttpReturnCode { get; set; }
    public List<ShortUrl>? Urls { get; set; }
    public string? ErrorMessage { get; set; }
}