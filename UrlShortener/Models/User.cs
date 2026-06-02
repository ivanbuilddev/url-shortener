using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Models;

public class User: IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<ShortUrl>? ShortUrls { get; set; }
}