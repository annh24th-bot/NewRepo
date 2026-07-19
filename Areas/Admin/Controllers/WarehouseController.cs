using System.Security.Claims;
using System.Text;
using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class WarehouseController : Controller
{
    private readonly AppDbContext _db;

    public WarehouseController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || p.Id.Contains(q));

        ViewBag.Query = q;
        return View(await query.OrderBy(p => p.Stock).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(string productId, string changeType, int quantity, string reason)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        int before = product.Stock;
        int change;

        switch (changeType)
        {
            case InventoryChangeType.In:
                product.Stock += Math.Abs(quantity);
                change = Math.Abs(quantity);
                break;
            case InventoryChangeType.Out:
                change = -Math.Min(Math.Abs(quantity), product.Stock);
                product.Stock += change;
                break;
            case InventoryChangeType.Set:
                change = quantity - product.Stock;
                product.Stock = Math.Max(0, quantity);
                break;
            default:
                return BadRequest();
        }

        _db.InventoryLogs.Add(new InventoryLog
        {
            ProductId = product.Id,
            ChangeType = changeType,
            QuantityChange = change,
            StockAfter = product.Stock,
            Reason = string.IsNullOrWhiteSpace(reason) ? "Không có ghi chú" : reason,
            PerformedBy = User.FindFirstValue(ClaimTypes.Email) ?? "system"
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã cập nhật tồn kho \"{product.Name}\": {before} → {product.Stock}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> History(string productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        var logs = await _db.InventoryLogs
            .Where(l => l.ProductId == productId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        ViewBag.Product = product;
        return View(logs);
    }

    public async Task<IActionResult> ExportCsv()
    {
        var products = await _db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Ma San Pham,Ten San Pham,Danh Muc,Ton Kho,Gia Ban");
        foreach (var p in products)
        {
            string categoryName = p.Category?.Name ?? p.CategoryId;
            sb.AppendLine($"\"{p.Id}\",\"{Csv(p.Name)}\",\"{Csv(categoryName)}\",{p.Stock},{p.Price}");
        }

        // UTF-8 BOM so Excel renders Vietnamese diacritics correctly.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"warehouse-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string value) => value.Replace("\"", "\"\"");
}
