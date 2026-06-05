using UrlShortener.Repositories;
using UrlShortener.DTOs;
using System.Net;
using UrlShortener.Models;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Extensions;

namespace UrlShortener.Services;

public class ClickInfoService : IClickInfoService
{
    private readonly IClickInfoRepository _clickInfoRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<IClickInfoService> _logger;

    public ClickInfoService(IClickInfoRepository clickInfoRepository, IDistributedCache cache, ILogger<IClickInfoService> logger)
    {
        _clickInfoRepository = clickInfoRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<List<UrlClickInfo>>> GetClickInfoByUrl(int urlId)
    {
        var cached = await _cache.GetObjectAsync<List<UrlClickInfo>>($"urlstats:{urlId}");
        if(cached != null) 
        {
            _logger.LogInformation("Get Info: hitting the cache");
            return Result<List<UrlClickInfo>>.Success(cached);
        }

        var clickInfo = await _clickInfoRepository.GetClickInfoByUrl(urlId);
        if (clickInfo == null)
        {
            return Result<List<UrlClickInfo>>.NotFound();
        }

        await _cache.SetObjectAsync<List<UrlClickInfo>>($"urlstats:{urlId}", clickInfo);
        return Result<List<UrlClickInfo>>.Success(clickInfo);
    }

    public async Task CreateClickInfo(CreateClickInfoRequest request)
    {
        await _clickInfoRepository.CreateClickInfo(request);
        await _cache.RemoveAsync($"urlstats:{request.ShortUrlId}");
    }
}