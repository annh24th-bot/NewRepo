using System.ComponentModel.DataAnnotations;

namespace DjStoreWeb.Models;

public class ContactMessage
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsHandled { get; set; } = false;

    [MaxLength(2000)]
    public string? AdminReply { get; set; }

    public DateTime? RepliedAt { get; set; }
}
