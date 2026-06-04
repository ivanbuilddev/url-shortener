using System.Net;
using UrlShortener.Models;

namespace UrlShortener.DTOs;

public class UrlClickInfoResponse
{
    public HttpStatusCode HttpReturnCode { get; set; }
    public List<UrlClickInfo>? ClickInfo { get; set; }
    public string? ErrorMessage { get; set; }
}