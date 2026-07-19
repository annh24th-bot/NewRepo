using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class NewsController : Controller
{
    private readonly AppDbContext _db;

    public NewsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.NewsPosts.OrderByDescending(n => n.CreatedAt).ToListAsync());
    }

    public IActionResult Create() => View(new NewsPost());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsPost model)
    {
        if (!ModelState.IsValid) return View(model);

        model.CreatedAt = DateTime.UtcNow;
        _db.NewsPosts.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã đăng bài viết mới.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var post = await _db.NewsPosts.FindAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NewsPost model)
    {
        var post = await _db.NewsPosts.FindAsync(id);
        if (post == null) return NotFound();

        post.Title = model.Title;
        post.Category = model.Category;
        post.Content = model.Content;
        post.Image = model.Image;
        post.Author = model.Author;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật bài viết.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _db.NewsPosts.FindAsync(id);
        if (post == null) return NotFound();

        _db.NewsPosts.Remove(post);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bài viết.";
        return RedirectToAction(nameof(Index));
    }
}
