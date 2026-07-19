using System.Security.Claims;
using DjStoreWeb.Data;
using DjStoreWeb.Models;
using DjStoreWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public CartController(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public IActionResult Index()
    {
        return View(_cart.GetItems());
    }

    [HttpPost]
    public async Task<IActionResult> Add(string productId, int quantity = 1)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        _cart.AddItem(product, quantity <= 0 ? 1 : quantity);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, count = _cart.TotalQuantity() });

        TempData["CartSuccess"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult UpdateQuantity(string productId, int quantity)
    {
        _cart.UpdateQuantity(productId, quantity);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, subtotal = _cart.Subtotal(), count = _cart.TotalQuantity() });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(string productId)
    {
        _cart.RemoveItem(productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var items = _cart.GetItems();
        if (items.Count == 0) return RedirectToAction(nameof(Index));

        var vm = new CheckoutViewModel
        {
            Items = items,
            Subtotal = _cart.Subtotal(),
            FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyVoucher(string voucherCode)
    {
        var subtotal = _cart.Subtotal();
        var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Code == voucherCode && v.IsActive);

        decimal discount = 0;
        string message = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
        bool ok = false;

        if (voucher != null
            && (voucher.ExpiryDate == null || voucher.ExpiryDate >= DateTime.UtcNow)
            && (voucher.UsageLimit == 0 || voucher.UsedCount < voucher.UsageLimit)
            && subtotal >= voucher.MinOrder)
        {
            discount = voucher.CalculateDiscount(subtotal);
            ok = true;
            message = $"Áp dụng mã \"{voucher.Code}\" thành công!";
        }

        return Json(new { success = ok, discount, total = subtotal - discount, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel form)
    {
        var items = _cart.GetItems();
        if (items.Count == 0) return RedirectToAction(nameof(Index));

        decimal subtotal = items.Sum(i => i.LineTotal);
        decimal discount = 0;
        Voucher? voucher = null;

        if (!string.IsNullOrWhiteSpace(form.VoucherCode))
        {
            voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Code == form.VoucherCode && v.IsActive);
            if (voucher != null && subtotal >= voucher.MinOrder
                && (voucher.ExpiryDate == null || voucher.ExpiryDate >= DateTime.UtcNow)
                && (voucher.UsageLimit == 0 || voucher.UsedCount < voucher.UsageLimit))
            {
                discount = voucher.CalculateDiscount(subtotal);
            }
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? form.Email ?? "guest@djstore.com";
        var order = new Order
        {
            OrderCode = $"DJS-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}",
            CustomerEmail = email.Trim().ToLowerInvariant(),
            CustomerName = string.IsNullOrWhiteSpace(form.FullName) ? (User.Identity?.Name ?? "Khách hàng") : form.FullName,
            Date = DateTime.UtcNow,
            Subtotal = subtotal,
            Discount = discount,
            VoucherCode = voucher?.Code,
            Total = subtotal - discount,
            Status = OrderStatus.Pending,
            PaymentMethod = string.IsNullOrWhiteSpace(form.PaymentMethod) ? "COD" : form.PaymentMethod,
            ShippingAddress = form.ShippingAddress ?? string.Empty,
            Phone = form.Phone ?? string.Empty
        };

        foreach (var item in items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                Image = item.Image
            });

            var product = await _db.Products.FindAsync(item.ProductId);
            if (product != null && product.Stock >= item.Quantity)
                product.Stock -= item.Quantity;
        }

        if (voucher != null)
            voucher.UsedCount += 1;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        _cart.Clear();

        return RedirectToAction(nameof(OrderSuccess), new { orderCode = order.OrderCode });
    }

    public async Task<IActionResult> OrderSuccess(string orderCode)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        if (order == null) return NotFound();

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        ViewBag.DriveLinks = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.DriveLink != null)
            .ToDictionaryAsync(p => p.Id, p => p.DriveLink!);

        return View(order);
    }
}

public class CheckoutViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ShippingAddress { get; set; }
    public string? PaymentMethod { get; set; }
    public string? VoucherCode { get; set; }
}
