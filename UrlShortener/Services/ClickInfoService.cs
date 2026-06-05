using UrlShortener.Repositories;
using UrlShortener.DTOs;
using System.Net;
using UrlShortener.Models;

namespace UrlShortener.Services;

public class ClickInfoService : IClickInfoService
{
    private readonly IClickInfoRepository _clickInfoRepository;

    public ClickInfoService(IClickInfoRepository clickInfoRepository)
    {
        _clickInfoRepository = clickInfoRepository;
    }

    public async Task<Result<List<UrlClickInfo>>> GetClickInfoByUrl(int urlId)
    {
        var clickInfo = await _clickInfoRepository.GetClickInfoByUrl(urlId);
        if (clickInfo == null)
        {
            return Result<List<UrlClickInfo>>.NotFound();
        }
        return Result<List<UrlClickInfo>>.Success(clickInfo);
    }

    public async Task CreateClickInfo(CreateClickInfoRequest request)
    {
        await _clickInfoRepository.CreateClickInfo(request);
    }
}