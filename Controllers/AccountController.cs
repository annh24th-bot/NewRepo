using System.Security.Claims;
using DjStoreWeb.Data;
using DjStoreWeb.Models;
using DjStoreWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive || !PasswordHasher.Verify(password ?? string.Empty, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (user.Role is UserRoles.Admin or UserRoles.Employee)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, string? phone, string? address)
    {
        email = (email ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ thông tin bắt buộc.");
            return View();
        }
        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
            return View();
        }
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
            return View();
        }

        var user = new AppUser
        {
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Role = UserRoles.Customer,
            Name = name,
            Phone = phone ?? string.Empty,
            Address = address ?? string.Empty
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        // Always show a generic success message to avoid leaking which emails are registered.
        ViewBag.Sent = true;
        ViewBag.Exists = exists;
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();
        return View(user);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string name, string phone, string address)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();

        user.Name = name;
        user.Phone = phone;
        user.Address = address;
        await _db.SaveChangesAsync();

        TempData["ProfileSuccess"] = "Cập nhật thông tin thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    public async Task<IActionResult> OrderHistory()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerEmail == email)
            .OrderByDescending(o => o.Date)
            .ToListAsync();

        var productIds = orders.SelectMany(o => o.Items).Select(i => i.ProductId).Distinct().ToList();
        ViewBag.DriveLinks = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.DriveLink != null)
            .ToDictionaryAsync(p => p.Id, p => p.DriveLink!);

        return View(orders);
    }
}
