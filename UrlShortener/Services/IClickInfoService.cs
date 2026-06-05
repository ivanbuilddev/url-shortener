using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Services;

public interface IClickInfoService
{
    public Task<Result<List<UrlClickInfo>>> GetClickInfoByUrl(int urlId);
    public Task CreateClickInfo(CreateClickInfoRequest request);
}