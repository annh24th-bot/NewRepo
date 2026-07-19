using DjStoreWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Controllers;

public class NewsController : Controller
{
    private readonly AppDbContext _db;

    public NewsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? category)
    {
        var query = _db.NewsPosts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(n => n.Category == category);

        ViewBag.Categories = await _db.NewsPosts.Select(n => n.Category).Distinct().ToListAsync();
        ViewBag.CurrentCategory = category;

        var posts = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        return View(posts);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var post = await _db.NewsPosts.FindAsync(id);
        if (post == null) return NotFound();

        post.Views += 1;
        await _db.SaveChangesAsync();

        ViewBag.Others = await _db.NewsPosts
            .Where(n => n.Id != id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(4)
            .ToListAsync();

        return View(post);
    }
}
