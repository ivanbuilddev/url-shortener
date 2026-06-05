using System.Net.Http.Headers;
using System.Net.Http.Json;
using UrlShortener.DTOs;
using UrlShortener.Models;
using System.Net;

namespace UrlShortener.Frontend.Services;

public class UrlService : IUrlService
{
    private readonly HttpClient _httpClient;
    private readonly IStorageService _storageService;

    public UrlService(HttpClient httpClient, IStorageService storageService)
    {
        _httpClient = httpClient;
        _storageService = storageService;
    }

    public async Task<Result<List<ShortUrl>>> GetAllUrls()
    {
        var token = await _storageService.GetItem("token");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("api/url/get-all-urls");
        var responseData = await response.Content.ReadFromJsonAsync<List<ShortUrl>>();
        
        if(response.StatusCode == HttpStatusCode.Unauthorized) return Result<List<ShortUrl>>.Unauthorized();
        if(response.StatusCode == HttpStatusCode.Forbidden) return Result<List<ShortUrl>>.Forbidden();
        if (!response.IsSuccessStatusCode) return Result<List<ShortUrl>>.Failure();

        return Result<List<ShortUrl>>.Success(responseData);
    }

    public async Task<Result<List<UrlClickInfo>>> GetUrlStats(int urlId)
    {
        var token = await _storageService.GetItem("token");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"api/url/click-info/{urlId}");
        var responseData = await response.Content.ReadFromJsonAsync<List<UrlClickInfo>>();
        
        if(response.StatusCode == HttpStatusCode.Unauthorized) return Result<List<UrlClickInfo>>.Unauthorized();
        if(response.StatusCode == HttpStatusCode.Forbidden) return Result<List<UrlClickInfo>>.Forbidden();
        if (!response.IsSuccessStatusCode) return Result<List<UrlClickInfo>>.Failure();

        return Result<List<UrlClickInfo>>.Success(responseData);
    }

    public async Task<Result<bool>> Create(CreateShortUrlRequest request)
    {
        var token = await _storageService.GetItem("token");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsJsonAsync("api/url/create", request);
        
        if(response.StatusCode == HttpStatusCode.Unauthorized) return Result<bool>.Unauthorized();
        if(response.StatusCode == HttpStatusCode.Forbidden) return Result<bool>.Forbidden();
        if (!response.IsSuccessStatusCode) return Result<bool>.Failure();

        return Result<bool>.Success(true);
    }
    public async Task<Result<bool>> Delete(int urlId)
    {
        var token = await _storageService.GetItem("token");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"api/url/delete/{urlId}");
        
        if(response.StatusCode == HttpStatusCode.Unauthorized) return Result<bool>.Unauthorized();
        if(response.StatusCode == HttpStatusCode.Forbidden) return Result<bool>.Forbidden();
        if (!response.IsSuccessStatusCode) return Result<bool>.Failure();

        return Result<bool>.Success(true);
    }
}