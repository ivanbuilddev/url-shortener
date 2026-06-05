using Microsoft.AspNetCore.Mvc;
using UrlShortener.DTOs;
using UrlShortener.Services;

namespace UrlShortener.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var response = await _userService.Register(request);
        return Ok(response.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var response = await _userService.Login(request);
        if(response.Status == System.Net.HttpStatusCode.Unauthorized) return Unauthorized(response.ErrorMessage);
        return Ok(response.Value);
    }
}