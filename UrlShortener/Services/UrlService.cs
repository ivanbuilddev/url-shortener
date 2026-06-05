using System.Text;
using UrlShortener.Repositories;
using System.Security.Cryptography;
using UrlShortener.Models;
using UrlShortener.DTOs;
using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Extensions;
using UrlShortener.Sockets;
using Microsoft.AspNetCore.SignalR;

namespace UrlShortener.Services;

public class UrlService : IUrlService
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly IUrlRepository _urlRepository;
    private readonly ILogger<UrlService> _logger;
    private readonly IDistributedCache _cache;

    public UrlService(IUrlRepository urlRepository, ILogger<UrlService> logger, IDistributedCache cache, IHubContext<DashboardHub> hubContext)
    {
        _urlRepository = urlRepository;
        _logger = logger;
        _cache = cache;
        _hubContext = hubContext;
    }

    public async Task<Result<List<ShortUrl>>> GetAllUrls(Guid userGuid)
    {
        _logger.LogInformation("Get: initialize get all urls");

        var cached = await _cache.GetObjectAsync<List<ShortUrl>>($"urlsbyuser:{userGuid}");
        if(cached != null) 
        {
            _logger.LogInformation("Get: hitting the cache");
            return Result<List<ShortUrl>>.Success(cached);
        }

        var urls = await _urlRepository.GetUrlsByUserId(userGuid);
        if (urls == null)
        {
            _logger.LogInformation("Get: urls not found");
            return Result<List<ShortUrl>>.NotFound();
        }
        _logger.LogInformation("Get: urls found");
        await _cache.SetObjectAsync($"urlsbyuser:{userGuid}", urls);
        return Result<List<ShortUrl>>.Success(urls);
    }

    public async Task<Result<ShortUrl>> GetOriginalUrl(string slug)
    {
        _logger.LogInformation("Get: initialize redirect using {slug}", slug);
        var cached = await _cache.GetObjectAsync<ShortUrl>($"originalurl:{slug}");
        if(cached != null) 
        {
            _logger.LogInformation("Get: hitting the cache");
            return Result<ShortUrl>.Success(cached);
        }

        var url = await _urlRepository.GetUrlBySlug(slug);
        if (url == null) 
        {
            _logger.LogInformation("Get: url not found");
            return Result<ShortUrl>.NotFound();
        }
        if(IsUrlExpired(url)) 
        {
            _logger.LogInformation("Get: url expired");
            return Result<ShortUrl>.Gone();
        }
        _logger.LogInformation("Get: url found");

        await _cache.SetObjectAsync($"originalurl:{slug}", url);

        return Result<ShortUrl>.Success(url);
    }

    public async Task<Result<ShortUrl>> CreateShortUrl(Guid userGuid, CreateShortUrlRequest request)
    {
        var originalUrl = request.OriginalUrl;
        var aliasUrl = request.AliasUrl;
        var validation = await ValidateExistsUrl(originalUrl);
        if(validation.Status == HttpStatusCode.OK) return validation;

        if(string.IsNullOrEmpty(aliasUrl))
        {
            ShortUrl url = await _urlRepository.CreateShortUrl(userGuid, request);
            string shortCode = GenerateShortCode(url.Id, originalUrl);

            while(await _urlRepository.ExistsShortUrl(shortCode))
            {
                shortCode = GenerateShortCode(url.Id, originalUrl);
            }
            url.Slug = shortCode;

            ShortUrl result = await _urlRepository.UpdateShortUrl(url);
            return Result<ShortUrl>.Success(result);
        }
        else
        {
            if(await _urlRepository.ExistsShortUrl(aliasUrl)) return Result<ShortUrl>.Conflict();
        
            ShortUrl url = await _urlRepository.CreateShortUrl(userGuid, request);

            return Result<ShortUrl>.Success(url);
        }
    }

    private async Task<Result<ShortUrl>> ValidateExistsUrl(string originalUrl)
    {
        List<ShortUrl>? shortUrlListCheck = await _urlRepository.GetUrlsByOriginalUrl(originalUrl);
        if (shortUrlListCheck != null)
        {
            foreach (ShortUrl possibleUrl in shortUrlListCheck)
            {
                if(!IsUrlExpired(possibleUrl)) return Result<ShortUrl>.Success(possibleUrl);
            }
        }

        return Result<ShortUrl>.NotFound();
    }

    public async Task UpdateCountUrl(string slug)
    {
        var url = await _urlRepository.GetUrlBySlug(slug);
        if(url == null) return;
        url.Clicks++;
        await _urlRepository.UpdateShortUrl(url);
        await _hubContext.Clients.All.SendAsync("UpdateUrlCounter", new DashboardSocket { UrlId = url.Id, NewCount = url.Clicks });
        return;
    }

    public async Task<bool> Delete(int urlId)
    {
        return await _urlRepository.DeleteShortUrl(urlId);
    }

    public async Task<bool> CheckIfImOwner(Guid userGuid, int urlId)
    {
        return await _urlRepository.CheckIfImOwner(userGuid, urlId);
    }

    private string GenerateShortCode(int id, string originalUrl)
    {
        string guid = Guid.NewGuid().ToString("N");

        string raw = id.ToString() + guid + originalUrl;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        var bigInt = Math.Abs(BitConverter.ToInt64(hash, 0));
        return ToBase62(bigInt);
    }

    private string ToBase62(long number)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var result = new StringBuilder();

        while(number > 0)
        {
            result.Insert(0, chars[(int)(number % 62)]);
            number /= 62;
        }
        
        return result.ToString();
    }

    private bool IsUrlExpired(ShortUrl url)
    {
        return url.ExpiryDate < DateTime.Now || (url.ClickLimit > 0 && url.Clicks >= url.ClickLimit);
    }
}