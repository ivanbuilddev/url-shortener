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

    public async Task<GetUrlResponse> GetOriginalUrl(string slug)
    {
        var url = await _urlRepository.GetUrlBySlug(slug);
        if (url == null) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.NotFound, ErrorMessage = "Url not found" };
        if(url.ExpiryDate < DateTime.Now) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.Gone, ErrorMessage = "Link expired" };
        if(url.ClickLimit > 0 && url.Clicks >= url.ClickLimit) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.TooManyRequests, ErrorMessage = "Link limit reached" };
        return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, OriginalUrl = url.OriginalUrl, Slug = url.Slug };
    }

    public async Task<GetUrlResponse> CreateShortUrl(CreateShortUrlRequest request)
    {
        var originalUrl = request.OriginalUrl;
        var aliasUrl = request.AliasUrl;
        var validation = await ValidateExistsUrl(originalUrl);
        if(validation.HttpReturnCode == HttpStatusCode.OK) return validation;

        if(string.IsNullOrEmpty(aliasUrl))
        {
            ShortUrl url = await _urlRepository.CreateShortUrl(request);
            string shortCode = GenerateShortCode(url.Id, originalUrl);

            while(await _urlRepository.ExistsShortUrl(shortCode))
            {
                shortCode = GenerateShortCode(url.Id, originalUrl);
            }
            url.Slug = shortCode;

            var result = await _urlRepository.UpdateShortUrl(url);
            return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, Slug = result.Slug };
        }
        else
        {
            if(await _urlRepository.ExistsShortUrl(aliasUrl)) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.Conflict, ErrorMessage = "Alias url already exists" };
        
            ShortUrl url = await _urlRepository.CreateShortUrl(request);

            return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, Slug = url.Slug };
        }
    }

    private async Task<GetUrlResponse> ValidateExistsUrl(string originalUrl)
    {
        List<ShortUrl>? shortUrlListCheck = await _urlRepository.GetUrlsByOriginalUrl(originalUrl);
        if (shortUrlListCheck != null)
        {
            foreach (ShortUrl possibleUrl in shortUrlListCheck)
            {
                if(possibleUrl.ExpiryDate <= DateTime.Now) return new GetUrlResponse { HttpReturnCode = HttpStatusCode.OK, Slug = possibleUrl.Slug };
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
}