using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DjStoreWeb.Models;

public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string Processed = "Processed";
    public const string Shipping = "Shipping";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string OrderCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }

    [MaxLength(60)]
    public string? VoucherCode { get; set; }
    public decimal Total { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = OrderStatus.Pending;

    [MaxLength(60)]
    public string PaymentMethod { get; set; } = "COD";

    [MaxLength(400)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    [MaxLength(120)]
    public string ProductId { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    [MaxLength(600)]
    public string Image { get; set; } = string.Empty;
}
