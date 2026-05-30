using System.Text;
using UrlShortener.Repositories;
using System.Security.Cryptography;
using UrlShortener.Models;

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

    public async Task<string> GetShortUrl(string originalUrl)
    {
        return await _urlRepository.GetShortUrl(originalUrl);
    }

    public async Task<string> GetOriginalUrl(string slug)
    {
        return await _urlRepository.GetOriginalUrl(slug);
    }

    public async Task<string> CreateShortUrl(string originalUrl)
    {
        string shortUrlCheck = await _urlRepository.GetShortUrl(originalUrl);
        if (!string.IsNullOrEmpty(shortUrlCheck))
        {
            return shortUrlCheck;
        }

        ShortUrl url = await _urlRepository.CreateShortUrl(originalUrl);
        string shortCode = GenerateShortCode(url.Id, originalUrl);

        while(await _urlRepository.ExistsShortUrl(shortCode))
        {
            shortCode = GenerateShortCode(url.Id, originalUrl);
        }
        url.Slug = shortCode;

        var result = await _urlRepository.UpdateShortUrl(url);
        return result.Slug;
    }

    public async Task UpdateCountUrl(string slug)
    {
        var url = await _urlRepository.GetUrl(slug);
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