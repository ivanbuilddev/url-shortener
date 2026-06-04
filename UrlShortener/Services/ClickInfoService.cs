using UrlShortener.Repositories;
using UrlShortener.DTOs;
using System.Net;

namespace UrlShortener.Services;

public class ClickInfoService : IClickInfoService
{
    private readonly IClickInfoRepository _clickInfoRepository;

    public ClickInfoService(IClickInfoRepository clickInfoRepository)
    {
        _clickInfoRepository = clickInfoRepository;
    }

    public async Task<UrlClickInfoResponse> GetClickInfoByUrl(int urlId)
    {
        var clickInfo = await _clickInfoRepository.GetClickInfoByUrl(urlId);
        if (clickInfo == null)
        {
            return new UrlClickInfoResponse { HttpReturnCode = HttpStatusCode.NotFound, ErrorMessage = "Click info not found" };
        }
        return new UrlClickInfoResponse { HttpReturnCode = HttpStatusCode.OK, ClickInfo = clickInfo };
    }

    public async Task CreateClickInfo(CreateClickInfoRequest request)
    {
        await _clickInfoRepository.CreateClickInfo(request);
    }
}