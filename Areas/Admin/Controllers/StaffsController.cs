using DjStoreWeb.Data;
using DjStoreWeb.Models;
using DjStoreWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class StaffsController : Controller
{
    private readonly AppDbContext _db;

    public StaffsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var staffs = await _db.Users
            .Where(u => u.Role == UserRoles.Employee || u.Role == UserRoles.Admin)
            .OrderBy(u => u.Role)
            .ToListAsync();
        return View(staffs);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string email, string password, string phone, string role)
    {
        email = (email ?? string.Empty).Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(string.Empty, "Email đã tồn tại.");
            return View();
        }

        var user = new AppUser
        {
            Name = name,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Phone = phone,
            Role = role == UserRoles.Admin ? UserRoles.Admin : UserRoles.Employee
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã tạo tài khoản nhân viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
