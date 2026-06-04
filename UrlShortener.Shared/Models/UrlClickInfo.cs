using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models;

public class UrlClickInfo
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int ShortUrlId { get; set; }
    public ShortUrl? ShortUrl { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Browser { get; set; }
    public string? BrowserVersion { get; set; }
    public string? OperatingSystem { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }
    public string? CountryCode { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.Now;
}