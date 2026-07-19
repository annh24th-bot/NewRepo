using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? status, string? q)
    {
        var query = _db.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(o => o.OrderCode.Contains(q) || o.CustomerEmail.Contains(q) || o.CustomerName.Contains(q));

        ViewBag.Status = status;
        ViewBag.Query = q;
        ViewBag.Statuses = new[] { OrderStatus.Pending, OrderStatus.Processed, OrderStatus.Shipping, OrderStatus.Delivered, OrderStatus.Cancelled };

        return View(await query.OrderByDescending(o => o.Date).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        ViewBag.Statuses = new[] { OrderStatus.Pending, OrderStatus.Processed, OrderStatus.Shipping, OrderStatus.Delivered, OrderStatus.Cancelled };
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng {order.OrderCode} thành \"{status}\".";
        return RedirectToAction(nameof(Details), new { id });
    }
}
