using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Data;
using UrlShortener.Models;

namespace Tests;

public class GetTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GetTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
           AllowAutoRedirect = false
        });
    }

    public async Task InitializeAsync()
    {
        // Wipe DB before each test
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenUrlNotFound()
    {
        var response = await _client.GetAsync("/nonexistent");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnGone_WhenUrlExpired()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "expired",
            OriginalUrl = "https://google.com",
            ExpiryDate = DateTime.Now.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/expired");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnRedirect_WhenUrlCorrect()
    {
        var url = "https://google.com";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url,
            ExpiryDate = DateTime.Now.AddDays(1),
            Clicks = 0
        });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/correct");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal(url + "/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Get_ShouldReturnRedirect_WhenUrlCorrectButDuplicated_1()
    {
        var url = "https://google.com";
        var url2 = "https://youtube.com";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url,
            ExpiryDate = DateTime.Now.AddDays(1),
            Clicks = 0
        });
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url2,
            ExpiryDate = DateTime.Now.AddDays(-1),
            Clicks = 0
        });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/correct");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal(url + "/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Get_ShouldReturnRedirect_WhenUrlCorrectButDuplicated_2()
    {
        var url = "https://google.com";
        var url2 = "https://youtube.com";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url2,
            ExpiryDate = DateTime.Now.AddDays(1),
            Clicks = 0
        });
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url,
            ExpiryDate = DateTime.Now.AddDays(-1),
            Clicks = 0
        });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/correct");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal(url2 + "/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Get_ShouldReturnGone_WhenExpiryDateEvenIfDuplicate_2()
    {
        var url = "https://google.com";
        var url2 = "https://youtube.com";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url2,
            ExpiryDate = DateTime.Now.AddDays(-1),
            Clicks = 0
        });
        db.ShortUrls.Add(new ShortUrl
        {
            Slug = "correct",
            OriginalUrl = url,
            ExpiryDate = DateTime.Now.AddDays(1),
            Clicks = 0
        });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/correct");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }
}
