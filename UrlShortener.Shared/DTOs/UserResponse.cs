using System.Net;

namespace UrlShortener.DTOs;

public class UserResponse
{
    public HttpStatusCode HttpReturnCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}