using System.Text.Json;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Http;

namespace DjStoreWeb.Services;

/// <summary>
/// Cart stored in the user's session as JSON. Works for both anonymous
/// and logged-in users, mirroring the original site's localStorage cart.
/// </summary>
public class CartService
{
    private const string SessionKey = "DjStoreCart";
    private readonly IHttpContextAccessor _accessor;

    public CartService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ISession Session => _accessor.HttpContext!.Session;

    public List<CartItem> GetItems()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new List<CartItem>();
        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void Save(List<CartItem> items)
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }

    public void AddItem(Product product, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Image = product.Image,
                Quantity = quantity
            });
        }
        Save(items);
    }

    public void UpdateQuantity(string productId, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId);
        if (existing == null) return;

        if (quantity <= 0)
            items.Remove(existing);
        else
            existing.Quantity = quantity;

        Save(items);
    }

    public void RemoveItem(string productId)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId);
        Save(items);
    }

    public void Clear()
    {
        Session.Remove(SessionKey);
    }

    public decimal Subtotal() => GetItems().Sum(i => i.LineTotal);

    public int TotalQuantity() => GetItems().Sum(i => i.Quantity);
}
