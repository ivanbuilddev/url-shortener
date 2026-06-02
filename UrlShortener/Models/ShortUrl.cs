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
    [Range(1, int.MaxValue, ErrorMessage = "Value must be greater than 0")]
    public int ClickLimit { get; set; } = -1;
    public bool IsEnabled { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public List<UrlClickInfo>? ClicksInfo { get; set; }
}