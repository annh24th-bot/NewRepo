using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // SQLite has no native decimal type, so SUM(decimal) can't be translated to SQL.
        // Pull the relevant order totals into memory first, then sum them there.
        var completedTotals = await _db.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .Select(o => o.Total)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalRevenue = completedTotals.Sum(),
            TotalOrders = await _db.Orders.CountAsync(),
            TotalProducts = await _db.Products.CountAsync(),
            TotalCustomers = await _db.Users.CountAsync(u => u.Role == UserRoles.Customer),
            PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
            LowStockProducts = await _db.Products.Where(p => p.Stock <= 5).OrderBy(p => p.Stock).Take(6).ToListAsync(),
            RecentOrders = await _db.Orders.OrderByDescending(o => o.Date).Take(8).ToListAsync(),
            UnhandledMessages = await _db.ContactMessages.CountAsync(m => !m.IsHandled),
            UnrepliedReviews = await _db.Reviews.CountAsync(r => r.AdminReply == null)
        };

        var last7 = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .OrderBy(d => d)
            .ToList();

        var ordersInRange = await _db.Orders
            .Where(o => o.Date >= last7.First())
            .ToListAsync();

        vm.RevenueLabels = last7.Select(d => d.ToString("dd/MM")).ToList();
        vm.RevenueValues = last7
            .Select(d => ordersInRange.Where(o => o.Date.Date == d && o.Status != OrderStatus.Cancelled).Sum(o => o.Total))
            .ToList();

        return View(vm);
    }
}

public class DashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingOrders { get; set; }
    public List<Product> LowStockProducts { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();
    public List<string> RevenueLabels { get; set; } = new();
    public List<decimal> RevenueValues { get; set; } = new();
    public int UnhandledMessages { get; set; }
    public int UnrepliedReviews { get; set; }
}
