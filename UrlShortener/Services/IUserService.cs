using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IUserService
{
    Task<Result<string>> Register(RegisterUserRequest request);
    Task<Result<string>> Login(LoginUserRequest request);
}