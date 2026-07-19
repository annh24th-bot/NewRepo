using DjStoreWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class ReviewsController : Controller
{
    private readonly AppDbContext _db;

    public ReviewsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? filter)
    {
        var query = _db.Reviews.Include(r => r.Product).AsQueryable();

        if (filter == "unreplied")
            query = query.Where(r => r.AdminReply == null);

        ViewBag.Filter = filter;
        ViewBag.UnrepliedCount = await _db.Reviews.CountAsync(r => r.AdminReply == null);

        return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string adminReply)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        review.AdminReply = adminReply;
        review.RepliedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã đăng phản hồi cho đánh giá.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã xóa đánh giá.";
        return RedirectToAction(nameof(Index));
    }
}
