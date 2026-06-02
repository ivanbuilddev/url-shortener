using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.DTOs;
using UrlShortener.Models;
using UrlShortener.Repositories;

namespace UrlShortener.Services;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IUserRepository _userRepository;

    public UserService(IConfiguration configuration, IPasswordHasher<string> passwordHasher, IUserRepository userRepository)
    {
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Register(RegisterUserRequest request)
    {
        User user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword("", request.Password)
        };

        await _userRepository.CreateUser(user);
        var token = GenerateToken(user.Id.ToString(), user.Email, user.UserName);
        return new UserResponse { HttpReturnCode = HttpStatusCode.OK, Token = token };
    }

    public async Task<UserResponse> Login(LoginUserRequest request)
    {
        User? user = await _userRepository.GetUserByEmailOrUsername(request.EmailOrUsername);
        if(user == null) return new UserResponse { HttpReturnCode = HttpStatusCode.Unauthorized, ErrorMessage = "Invalid email or password" };
        var verifyResult = _passwordHasher.VerifyHashedPassword("", user.PasswordHash, request.Password);
        if(verifyResult == PasswordVerificationResult.Failed) return new UserResponse { HttpReturnCode = HttpStatusCode.Unauthorized, ErrorMessage = "Invalid email or password" };
        
        var token = GenerateToken(user.Id.ToString(), user.Email, user.UserName);
        return new UserResponse { HttpReturnCode = HttpStatusCode.OK, Token = token };
    }

    private string GenerateToken(string userId, string email, string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}