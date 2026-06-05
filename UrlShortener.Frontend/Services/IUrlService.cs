using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Frontend.Services;

public interface IUrlService
{
    public Task<Result<List<ShortUrl>>> GetAllUrls();
    public Task<Result<List<UrlClickInfo>>> GetUrlStats(int urlId);
    public Task<Result<bool>> Create(CreateShortUrlRequest request);
    public Task<Result<bool>> Delete(int urlId);

}