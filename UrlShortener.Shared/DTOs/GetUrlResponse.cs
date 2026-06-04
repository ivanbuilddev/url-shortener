using System.Net;
using UrlShortener.Models;

namespace UrlShortener.DTOs;

public class GetUrlResponse
{
    public HttpStatusCode HttpReturnCode { get; set; }
    public ShortUrl ShortUrl { get; set; } = new ShortUrl();
    public string ErrorMessage { get; set; } = string.Empty;
}