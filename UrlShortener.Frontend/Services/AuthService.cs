using System.Net.Http.Json;
using UrlShortener.DTOs;

namespace UrlShortener.Frontend.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> Login(string emailOrUsername, string password)
    {
        LoginUserRequest request = new LoginUserRequest
        {
            EmailOrUsername = emailOrUsername,
            Password = password
        };
        var response = await _httpClient.PostAsJsonAsync("api/user/login", request);

        var responseData = await response.Content.ReadAsStringAsync();
        return responseData;
    }

    public async Task<string> Register(string username, string email, string password)
    {
        RegisterUserRequest request = new RegisterUserRequest
        {
            Username = username,
            Email = email,
            Password = password
        };
        var response = await _httpClient.PostAsJsonAsync("api/user/register", request);

        var responseData = await response.Content.ReadAsStringAsync();
        return responseData;
    }
}