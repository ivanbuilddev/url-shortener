namespace UrlShortener.DTOs;

public class LoginUserRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}