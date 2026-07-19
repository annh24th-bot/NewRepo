using System.ComponentModel.DataAnnotations;

namespace DjStoreWeb.Models;

public class NewsPost
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "Tin tức";

    public string Content { get; set; } = string.Empty;

    [MaxLength(600)]
    public string Image { get; set; } = string.Empty;

    public int Views { get; set; } = 0;

    [MaxLength(150)]
    public string Author { get; set; } = "Admin";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
