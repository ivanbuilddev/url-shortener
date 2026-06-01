using System.Net;
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
        GetUrlResponse url = await _urlService.GetOriginalUrl(slug);
        if(url.HttpReturnCode == HttpStatusCode.NotFound) return NotFound(url.ErrorMessage);
        if(url.HttpReturnCode == HttpStatusCode.Gone) return StatusCode((int)HttpStatusCode.Gone, url.ErrorMessage);
        if(url.HttpReturnCode == HttpStatusCode.TooManyRequests) return StatusCode((int)HttpStatusCode.TooManyRequests, url.ErrorMessage);
        await _urlService.UpdateCountUrl(slug);
        return RedirectPermanent(url.OriginalUrl);
    }

    [HttpPost("create")]
    [EnableRateLimiting("create")]
    public async Task<IActionResult> Create(CreateShortUrlRequest request)
    {
        var response = await _urlService.CreateShortUrl(request);
        return Ok(new { response.Slug });
    }
}