using UrlShortener.DTOs;

namespace UrlShortener.Repositories;

public interface IClickInfoRepository
{
    public Task CreateClickInfo(CreateClickInfoRequest request);
}