using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DjStoreWeb.Models;

public static class InventoryChangeType
{
    public const string In = "In";       // nhập kho (+)
    public const string Out = "Out";     // xuất kho (-)
    public const string Set = "Set";     // thiết lập số lượng trực tiếp
}

public class InventoryLog
{
    [Key]
    public int Id { get; set; }

    [MaxLength(120)]
    public string ProductId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [MaxLength(10)]
    public string ChangeType { get; set; } = InventoryChangeType.In;

    public int QuantityChange { get; set; }

    public int StockAfter { get; set; }

    [MaxLength(400)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PerformedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ActivityLog
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string ActorEmail { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Action { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
