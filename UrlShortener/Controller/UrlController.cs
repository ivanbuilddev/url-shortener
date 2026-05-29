using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.Services;

namespace UrlShortener.Controller;

[ApiController]
[EnableRateLimiting("create")]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;

    public UrlController(IUrlService urlService)
    {
        _urlService = urlService;
    }

    public async Task<IActionResult> Get(string slug)
    {
        var url = await _urlService.GetShortUrl(slug);
        if (string.IsNullOrEmpty(url))
        {
            return NotFound();
        }
        return Ok(new { url });
    }

    [HttpPost("create")]
    [EnableRateLimiting("create")]
    public async Task<IActionResult> Create(string url)
    {
        var slug = await _urlService.CreateShortUrl(url);
        return Ok(new { slug });
    }
}