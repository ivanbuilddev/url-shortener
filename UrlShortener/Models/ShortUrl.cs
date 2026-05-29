using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models;

public class ShortUrl
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string OriginalUrl { get; set; } = string.Empty;
    [Required]
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ExpiryDate { get; set; } = DateTime.Now + TimeSpan.FromDays(1);
    public int Clicks { get; set; } = 0;
}