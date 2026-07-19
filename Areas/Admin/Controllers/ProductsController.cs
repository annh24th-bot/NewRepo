using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class ProductsController : Controller
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q, string? category)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || p.Id.Contains(q));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.CategoryId == category);

        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.Query = q;
        ViewBag.Category = category;

        return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.ToListAsync();
        return View(new Product { Id = Guid.NewGuid().ToString("N")[..10] });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        if (string.IsNullOrWhiteSpace(model.Id)) model.Id = Guid.NewGuid().ToString("N")[..10];
        if (await _db.Products.AnyAsync(p => p.Id == model.Id))
        {
            ModelState.AddModelError(string.Empty, "Mã sản phẩm đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(model.SpecsJson)) model.SpecsJson = "{}";
        _db.Products.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã tạo sản phẩm mới.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        ViewBag.Categories = await _db.Categories.ToListAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Product model)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }

        product.Name = model.Name;
        product.CategoryId = model.CategoryId;
        product.Brand = model.Brand;
        product.Price = model.Price;
        product.OriginalPrice = model.OriginalPrice;
        product.Image = model.Image;
        product.Description = model.Description;
        product.FlashSale = model.FlashSale;
        product.Discount = model.Discount;
        product.IsActive = model.IsActive;
        product.DriveLink = model.DriveLink;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa sản phẩm.";
        return RedirectToAction(nameof(Index));
    }
}
