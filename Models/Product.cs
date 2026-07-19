using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DjStoreWeb.Models;

public class Product
{
    [Key]
    [MaxLength(120)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string CategoryId { get; set; } = string.Empty;

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [MaxLength(150)]
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }

    public double Rating { get; set; }

    public int Reviews { get; set; }

    [MaxLength(600)]
    public string Image { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Serialized JSON dictionary of spec name/value pairs, e.g. {"Kênh":"2 kênh"}</summary>
    public string SpecsJson { get; set; } = "{}";

    public bool FlashSale { get; set; }

    [MaxLength(20)]
    public string? Discount { get; set; }

    /// <summary>Number of units currently in stock (managed via Warehouse module).</summary>
    public int Stock { get; set; } = 0;

    /// <summary>
    /// Google Drive link for digital products (music packs, software licenses, cloud storage).
    /// Revealed to the customer once their order is confirmed/processed.
    /// </summary>
    [MaxLength(600)]
    public string? DriveLink { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
