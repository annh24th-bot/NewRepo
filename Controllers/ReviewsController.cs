using System.Security.Claims;
using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Controllers;

public class ReviewsController : Controller
{
    private readonly AppDbContext _db;

    public ReviewsController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string productId, int rating, string comment)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["ReviewError"] = "Vui lòng nhập nội dung nhận xét.";
            return RedirectToAction("Product", "Home", new { id = productId });
        }

        var review = new Review
        {
            ProductId = productId,
            CustomerName = User.Identity?.Name ?? "Khách hàng",
            CustomerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá sản phẩm! Nhận xét của bạn đã được ghi nhận.";
        return RedirectToAction("Product", "Home", new { id = productId });
    }
}
