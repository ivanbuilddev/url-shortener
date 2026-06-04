using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Repositories;

public interface IClickInfoRepository
{
    public Task CreateClickInfo(CreateClickInfoRequest request);
    public Task<List<UrlClickInfo>?> GetClickInfoByUrl(int urlId);   
}