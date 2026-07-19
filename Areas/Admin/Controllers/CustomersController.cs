using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class CustomersController : Controller
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Users.Where(u => u.Role == UserRoles.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Name.Contains(q) || u.Email.Contains(q));

        ViewBag.Query = q;
        return View(await query.OrderByDescending(u => u.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var orders = await _db.Orders.Include(o => o.Items)
            .Where(o => o.CustomerEmail == user.Email)
            .OrderByDescending(o => o.Date)
            .ToListAsync();

        ViewBag.Orders = orders;
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = user.IsActive ? "Đã kích hoạt tài khoản." : "Đã khóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }
}
