using UrlShortener.Models;

namespace UrlShortener.Repositories;

public interface IUserRepository
{
    public Task<User?> GetUserByEmailOrUsername(string emailOrUsername);
    public Task<User> CreateUser(User user);
}