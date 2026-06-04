using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.DTOs;
using UrlShortener.Services;
using UAParser;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UrlShortener.Controller;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly IGeolocationService _geolocationService;
    private readonly IClickInfoService _clickInfoService;

    public UrlController(IUrlService urlService, IGeolocationService geolocationService, IClickInfoService clickInfoService)
    {
        _urlService = urlService;
        _geolocationService = geolocationService;
        _clickInfoService = clickInfoService;
    }

    [Authorize]
    [HttpGet("get-all-urls")]
    public async Task<IActionResult> Get()
    {
        string? userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdentifier == null) return Unauthorized();
        Guid userGuidString = Guid.Parse(userIdentifier);
        var urls = await _urlService.GetAllUrls(userGuidString);
        if(urls.HttpReturnCode == HttpStatusCode.NotFound) return StatusCode((int)HttpStatusCode.NotFound, urls.ErrorMessage);
        return Ok(urls.Urls);
    }

    [Authorize]
    [HttpGet("click-info/{id}")]
    public async Task<IActionResult> GetClickInfo(int id)
    {
        string? userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdentifier == null) return Unauthorized();
        Guid userGuidString = Guid.Parse(userIdentifier);
        if(await _urlService.CheckIfImOwner(userGuidString, id) == false) return Unauthorized();
        var clickInfo = await _clickInfoService.GetClickInfoByUrl(id);
        if(clickInfo.HttpReturnCode == HttpStatusCode.NotFound) return StatusCode((int)HttpStatusCode.NotFound, clickInfo.ErrorMessage);
        return Ok(clickInfo.ClickInfo);
    }

    [HttpGet("~/{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        GetUrlResponse url = await _urlService.GetOriginalUrl(slug);
        if(url.HttpReturnCode == HttpStatusCode.NotFound) return StatusCode((int)HttpStatusCode.NotFound, url.ErrorMessage);
        if(url.HttpReturnCode == HttpStatusCode.Gone) return StatusCode((int)HttpStatusCode.Gone, url.ErrorMessage);
        if(url.HttpReturnCode == HttpStatusCode.TooManyRequests) return StatusCode((int)HttpStatusCode.TooManyRequests, url.ErrorMessage);
        await _urlService.UpdateCountUrl(slug);

        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var uaParser = Parser.GetDefault();
        ClientInfo clientInfo = uaParser.Parse(userAgent);
        var ipAddress = GetClientIpAddress();

        var clickInfo = new CreateClickInfoRequest{
            ShortUrlId = url.ShortUrl.Id,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Browser = clientInfo.UA.Family,
            BrowserVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}",
            OperatingSystem = clientInfo.OS.Family,
            Referrer = HttpContext.Request.Headers["Referer"].ToString(),
            DeviceType = clientInfo.Device.Family,
            CountryCode = await _geolocationService.GetCountryCode(ipAddress),
        };
        await _clickInfoService.CreateClickInfo(clickInfo);
        return Redirect(url.ShortUrl.OriginalUrl);
    }

    [HttpPost("create")]
    [Authorize]
    [EnableRateLimiting("create")]
    public async Task<IActionResult> Create(CreateShortUrlRequest request)
    {
        string? userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdentifier == null) return Unauthorized();
        Guid userGuidString = Guid.Parse(userIdentifier);
        var response = await _urlService.CreateShortUrl(userGuidString, request);
        return Ok(new { response.ShortUrl.Slug });
    }

    [HttpDelete("delete/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        string? userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdentifier == null) return Unauthorized();
        Guid userGuidString = Guid.Parse(userIdentifier);
        if(await _urlService.CheckIfImOwner(userGuidString, id) == false) return Unauthorized();
        var response = await _urlService.Delete(id);
        return Ok(response);
    }

    private string GetClientIpAddress()
     {
        return Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? HttpContext.Connection.RemoteIpAddress?.ToString()
               ?? "Unknown";
    }
}