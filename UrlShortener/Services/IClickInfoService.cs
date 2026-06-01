using UrlShortener.DTOs;

namespace UrlShortener.Services;

public interface IClickInfoService
{
    public Task CreateClickInfo(CreateClickInfoRequest request);
}