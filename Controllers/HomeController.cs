using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.FlashSale = await _db.Products.Where(p => p.IsActive && p.FlashSale).OrderByDescending(p => p.CreatedAt).Take(8).ToListAsync();
        ViewBag.Featured = await _db.Products.Where(p => p.IsActive).OrderByDescending(p => p.Rating).Take(8).ToListAsync();
        ViewBag.News = await _db.NewsPosts.OrderByDescending(n => n.CreatedAt).Take(3).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Shop(string? category, string? q, string sort = "default", int page = 1)
    {
        const int pageSize = 12;
        var query = _db.Products.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.CategoryId == category);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || p.Brand.Contains(q) || p.Description.Contains(q));

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.Rating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        int total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.CurrentCategory = category;
        ViewBag.Query = q;
        ViewBag.Sort = sort;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(items);
    }

    public async Task<IActionResult> Product(string id)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        ViewBag.Related = await _db.Products
    .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
    .OrderByDescending(p => p.Rating)
    .Take(4)
    .ToListAsync();

        var reviews = await _db.Reviews
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.Reviews = reviews;
        // Fall back to the seeded rating/review-count when nobody has left a real review yet.
        ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : product.Rating;
        ViewBag.ReviewCount = reviews.Any() ? reviews.Count : product.Reviews;

        return View(product);
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(string name, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
        {
            TempData["ContactError"] = "Vui lòng điền đầy đủ thông tin.";
            return RedirectToAction(nameof(Contact));
        }

        _db.ContactMessages.Add(new ContactMessage
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Message = message.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["ContactSuccess"] = "Cảm ơn bạn đã liên hệ! Đội ngũ DJ Store sẽ phản hồi trong thời gian sớm nhất.";
        return RedirectToAction(nameof(Contact));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
