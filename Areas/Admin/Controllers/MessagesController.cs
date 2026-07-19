using DjStoreWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class MessagesController : Controller
{
    private readonly AppDbContext _db;

    public MessagesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? filter)
    {
        var query = _db.ContactMessages.AsQueryable();

        if (filter == "unhandled")
            query = query.Where(m => !m.IsHandled);

        ViewBag.Filter = filter;
        ViewBag.UnhandledCount = await _db.ContactMessages.CountAsync(m => !m.IsHandled);

        return View(await query.OrderByDescending(m => m.CreatedAt).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string adminReply)
    {
        var message = await _db.ContactMessages.FindAsync(id);
        if (message == null) return NotFound();

        message.AdminReply = adminReply;
        message.RepliedAt = DateTime.UtcNow;
        message.IsHandled = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã lưu phản hồi cho \"{message.Name}\".";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkHandled(int id)
    {
        var message = await _db.ContactMessages.FindAsync(id);
        if (message == null) return NotFound();

        message.IsHandled = !message.IsHandled;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
