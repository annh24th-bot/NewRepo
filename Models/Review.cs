using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DjStoreWeb.Models;

public class Review
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string ProductId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Reply written by admin/staff, shown publicly under the review.</summary>
    [MaxLength(1000)]
    public string? AdminReply { get; set; }

    public DateTime? RepliedAt { get; set; }
}
