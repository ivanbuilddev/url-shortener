using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.DTOs;
using UrlShortener.Services;
using UAParser;

namespace UrlShortener.Controller;

[ApiController]
[EnableRateLimiting("create")]
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
    [EnableRateLimiting("create")]
    public async Task<IActionResult> Create(CreateShortUrlRequest request)
    {
        var response = await _urlService.CreateShortUrl(request);
        return Ok(new { response.ShortUrl.Slug });
    }

    private string GetClientIpAddress()
     {
        return Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? HttpContext.Connection.RemoteIpAddress?.ToString()
               ?? "Unknown";
    }
}