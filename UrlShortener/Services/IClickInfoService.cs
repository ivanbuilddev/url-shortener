using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IClickInfoService
{
    public Task<UrlClickInfoResponse> GetClickInfoByUrl(int urlId);
    public Task CreateClickInfo(CreateClickInfoRequest request);
}