namespace UrlShortener.Frontend.Services;

public interface IAuthService
{
    public Task<string> Login(string usernameOrEmail, string password);
    public Task<string> Register(string username, string email, string password);
}