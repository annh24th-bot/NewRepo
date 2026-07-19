using System.Text.Json.Serialization;

namespace DjStoreWeb.Data.Seed;

public class CategorySeedDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ProductSeedDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string>? Specs { get; set; }
    public bool FlashSale { get; set; }
    public string? Discount { get; set; }
}

public class UserSeedDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
}

public class OrderItemSeedDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Image { get; set; } = string.Empty;
}

public class OrderSeedDto
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<OrderItemSeedDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public string? VoucherCode { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
