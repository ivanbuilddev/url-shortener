using System.Text;
using UrlShortener.Repositories;
using System.Security.Cryptography;
using UrlShortener.Models;
using UrlShortener.DTOs;
using System.Net;

namespace UrlShortener.Services;

public class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;
    private readonly ILogger<UrlService> _logger;

    public UrlService(IUrlRepository urlRepository, ILogger<UrlService> logger)
    {
        _urlRepository = urlRepository;
        _logger = logger;
    }

    public async Task<GetAllUrlsResponse> GetAllUrls(Guid userGuid)
    {
        _logger.LogInformation("Get: initialize get all urls");
        var urls = await _urlRepository.GetUrlsByUserId(userGuid);
        if (urls == null)
        {
            _logger.LogInformation("Get: urls not found");
            return new GetAllUrlsResponse { HttpReturnCode = HttpStatusCode.NotFound, ErrorMessage = "Urls not found" };
        }
        _logger.LogInformation("Get: urls found");
        return new GetAllUrlsResponse { HttpReturnCode = HttpStatusCode.OK, Urls = urls };
    }

    public async Task<GetUrlResponse> GetOriginalUrl(string slug)
    {
        _logger.LogInformation("Get: initialize redirect using {slug}", slug);
        var url = await _urlRepository.GetUrlBySlug(slug);
        if (url == null) 
        {
            _logger.LogInformation("Get: url not found");
            return new GetUrlResponse { HttpReturnCode = HttpStatusCode.NotFound, ErrorMessage = "Url not found" };
        }
        if(IsUrlExpired(url)) 
        {
            _logger.LogInformation("Get: url expired");
            return new GetUrlResponse { HttpReturnCode = HttpStatusCode.Gone, ErrorMessage = "Link expired" };
        }
        _logger.LogInformation("Get: url found");
        return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, ShortUrl = url };
    }

    public async Task<GetUrlResponse> CreateShortUrl(Guid userGuid, CreateShortUrlRequest request)
    {
        var originalUrl = request.OriginalUrl;
        var aliasUrl = request.AliasUrl;
        var validation = await ValidateExistsUrl(originalUrl);
        if(validation.HttpReturnCode == HttpStatusCode.OK) return validation;

        if(string.IsNullOrEmpty(aliasUrl))
        {
            ShortUrl url = await _urlRepository.CreateShortUrl(userGuid, request);
            string shortCode = GenerateShortCode(url.Id, originalUrl);

            while(await _urlRepository.ExistsShortUrl(shortCode))
            {
                shortCode = GenerateShortCode(url.Id, originalUrl);
            }
            url.Slug = shortCode;

            ShortUrl result = await _urlRepository.UpdateShortUrl(url);
            return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, ShortUrl = result };
        }
        else
        {
            if(await _urlRepository.ExistsShortUrl(aliasUrl)) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.Conflict, ErrorMessage = "Alias url already exists" };
        
            ShortUrl url = await _urlRepository.CreateShortUrl(userGuid, request);

            return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, ShortUrl = url };
        }
    }

    private async Task<GetUrlResponse> ValidateExistsUrl(string originalUrl)
    {
        List<ShortUrl>? shortUrlListCheck = await _urlRepository.GetUrlsByOriginalUrl(originalUrl);
        if (shortUrlListCheck != null)
        {
            foreach (ShortUrl possibleUrl in shortUrlListCheck)
            {
                if(!IsUrlExpired(possibleUrl)) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, ShortUrl = possibleUrl };
            }
        }

        return new GetUrlResponse { HttpReturnCode = HttpStatusCode.NotFound, ErrorMessage = "Url not found" };
    }

    public async Task UpdateCountUrl(string slug)
    {
        var url = await _urlRepository.GetUrlBySlug(slug);
        if(url == null) return;
        url.Clicks++;
        await _urlRepository.UpdateShortUrl(url);
        return;
    }

    public async Task<bool> Delete(int urlId)
    {
        return await _urlRepository.DeleteShortUrl(urlId);
    }

    public async Task<bool> CheckIfImOwner(Guid userGuid, int urlId)
    {
        return await _urlRepository.CheckIfImOwner(userGuid, urlId);
    }

    private string GenerateShortCode(int id, string originalUrl)
    {
        string guid = Guid.NewGuid().ToString("N");

        string raw = id.ToString() + guid + originalUrl;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        var bigInt = Math.Abs(BitConverter.ToInt64(hash, 0));
        return ToBase62(bigInt);
    }

    private string ToBase62(long number)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var result = new StringBuilder();

        while(number > 0)
        {
            result.Insert(0, chars[(int)(number % 62)]);
            number /= 62;
        }
        
        return result.ToString();
    }

    private bool IsUrlExpired(ShortUrl url)
    {
        return url.ExpiryDate < DateTime.Now || (url.ClickLimit > 0 && url.Clicks >= url.ClickLimit);
    }
}