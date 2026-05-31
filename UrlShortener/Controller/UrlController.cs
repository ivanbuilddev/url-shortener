using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.DTOs;
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

    [HttpGet("~/{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        string url = await _urlService.GetOriginalUrl(slug);
        if (string.IsNullOrEmpty(url))
        {
            return NotFound();
        }
        await _urlService.UpdateCountUrl(slug);
        return RedirectPermanent(url);
    }

    [HttpPost("create")]
    [EnableRateLimiting("create")]
    public async Task<IActionResult> Create(CreateShortUrlRequest request)
    {
        var slug = "";
        if(string.IsNullOrEmpty(request.AliasUrl))
            slug = await _urlService.CreateShortUrl(request.OriginalUrl);
        else
            slug = await _urlService.CreateShortUrl(request.OriginalUrl, request.AliasUrl);
        return Ok(new { slug });
    }
}