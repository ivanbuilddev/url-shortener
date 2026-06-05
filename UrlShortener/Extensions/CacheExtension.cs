using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace UrlShortener.Extensions;

public static class CacheExtension
{
    
    private static readonly JsonSerializerOptions _options = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
    private static readonly DistributedCacheEntryOptions _optionsCache = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    };

    public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions? options = null)
    {
        string serializeValue = JsonSerializer.Serialize(value, _options);
        await cache.SetStringAsync(key, serializeValue, options ?? _optionsCache);
    }

    public static async Task<T?> GetObjectAsync<T>(this IDistributedCache cache, string key)
    {
        var cached = await cache.GetStringAsync(key);
        if (cached == null) return default;
        return JsonSerializer.Deserialize<T>(cached, _options);
    }
}