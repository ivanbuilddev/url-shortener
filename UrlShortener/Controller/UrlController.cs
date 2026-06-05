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
        if(urls.Status == HttpStatusCode.NotFound) return StatusCode((int)HttpStatusCode.NotFound, urls.ErrorMessage);
        return Ok(urls.Value);
    }

    [Authorize]
    [HttpGet("click-info/{id}")]
    public async Task<IActionResult> GetClickInfo(int id)
    {
        string? userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdentifier == null) return Unauthorized();
        Guid userGuidString = Guid.Parse(userIdentifier);
        if(await _urlService.CheckIfImOwner(userGuidString, id) == false) return Forbid();
        var clickInfo = await _clickInfoService.GetClickInfoByUrl(id);
        if(clickInfo.Status == HttpStatusCode.NotFound) return StatusCode((int)HttpStatusCode.NotFound, clickInfo.ErrorMessage);
        return Ok(clickInfo.Value);
    }

    [HttpGet("~/{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        var url = await _urlService.GetOriginalUrl(slug);
        if(url.Status == HttpStatusCode.NotFound || url.Value == null) return StatusCode((int)HttpStatusCode.NotFound, url.ErrorMessage);
        if(url.Status == HttpStatusCode.Gone) return StatusCode((int)HttpStatusCode.Gone, url.ErrorMessage);
        await _urlService.UpdateCountUrl(slug);

        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var uaParser = Parser.GetDefault();
        ClientInfo clientInfo = uaParser.Parse(userAgent);
        var ipAddress = GetClientIpAddress();

        var clickInfo = new CreateClickInfoRequest{
            ShortUrlId = url.Value.Id,
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
        return Redirect(url.Value.OriginalUrl);
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
        if(response.Status == HttpStatusCode.Conflict) return Conflict("slug already exists");
        if(response.Value == null) return UnprocessableEntity("Can not create resource");
        return StatusCode((int)HttpStatusCode.Created, response.Value);
    }

    [HttpDelete("delete/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        string? userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdentifier == null) return Unauthorized();
        Guid userGuidString = Guid.Parse(userIdentifier);
        if(await _urlService.CheckIfImOwner(userGuidString, id) == false) return Forbid();
        await _urlService.Delete(id);
        return NoContent();
    }

    private string GetClientIpAddress()
     {
        return Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? HttpContext.Connection.RemoteIpAddress?.ToString()
               ?? "Unknown";
    }
}