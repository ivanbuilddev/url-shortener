using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IUserService
{
    Task<UserResponse> Register(RegisterUserRequest request);
    Task<UserResponse> Login(LoginUserRequest request);
}