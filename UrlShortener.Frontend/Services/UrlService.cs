using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UrlShortener.DTOs;
using UrlShortener.Models;

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

    public async Task<List<ShortUrl>?> GetAllUrls()
    {
        var token = await _storageService.GetItem("token");
        if(string.IsNullOrEmpty(token)) return null;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("api/url/get-all-urls");
        var responseData = await response.Content.ReadFromJsonAsync<List<ShortUrl>>();
        
        if (!response.IsSuccessStatusCode) return null;

        return responseData;
    }

    public async Task<List<UrlClickInfo>?> GetUrlStats(int urlId)
    {
        var token = await _storageService.GetItem("token");
        if(string.IsNullOrEmpty(token)) return null;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"api/url/click-info/{urlId}");
        var responseData = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode) return null;

        return JsonSerializer.Deserialize<List<UrlClickInfo>>(responseData);
    }

    public async Task<bool> Create(CreateShortUrlRequest request)
    {
        var token = await _storageService.GetItem("token");
        if(string.IsNullOrEmpty(token)) return false;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsJsonAsync("api/url/create", request);
        
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> Delete(int urlId)
    {
        var token = await _storageService.GetItem("token");
        if(string.IsNullOrEmpty(token)) return false;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"api/url/delete/{urlId}");
        
        return response.IsSuccessStatusCode;
    }
}