using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<ShortUrl>? ShortUrls { get; set; }
}