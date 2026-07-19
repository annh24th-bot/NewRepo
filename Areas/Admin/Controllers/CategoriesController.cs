using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class CategoriesController : Controller
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .Select(c => c)
            .ToListAsync();

        var counts = await _db.Products.GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        ViewBag.ProductCounts = counts;
        return View(categories);
    }

    public IActionResult Create() => View(new Category());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập mã danh mục.");
        }
        else if (await _db.Categories.AnyAsync(c => c.Id == model.Id))
        {
            ModelState.AddModelError(string.Empty, "Mã danh mục đã tồn tại.");
        }

        if (!ModelState.IsValid) return View(model);

        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo danh mục.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Category model)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        category.Name = model.Name;
        category.Icon = model.Icon;
        category.Description = model.Description;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã cập nhật danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            TempData["Error"] = "Không thể xóa danh mục đang có sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa danh mục.";
        return RedirectToAction(nameof(Index));
    }
}
