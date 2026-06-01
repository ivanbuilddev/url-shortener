using UrlShortener.Repositories;
using UrlShortener.DTOs;

namespace UrlShortener.Services;

public class ClickInfoService : IClickInfoService
{
    private readonly IClickInfoRepository _clickInfoRepository;

    public ClickInfoService(IClickInfoRepository clickInfoRepository)
    {
        _clickInfoRepository = clickInfoRepository;
    }

    public async Task CreateClickInfo(CreateClickInfoRequest request)
    {
        await _clickInfoRepository.CreateClickInfo(request);
    }
}