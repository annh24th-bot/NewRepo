using DjStoreWeb.Data;
using DjStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DjStoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin,employee")]
public class VouchersController : Controller
{
    private readonly AppDbContext _db;

    public VouchersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.Vouchers.OrderByDescending(v => v.ExpiryDate).ToListAsync());
    }

    public IActionResult Create() => View(new Voucher());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Voucher model)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        if (await _db.Vouchers.AnyAsync(v => v.Code == model.Code))
        {
            ModelState.AddModelError(string.Empty, "Mã voucher đã tồn tại.");
            return View(model);
        }

        _db.Vouchers.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo mã giảm giá.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var voucher = await _db.Vouchers.FindAsync(id);
        if (voucher == null) return NotFound();
        return View(voucher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Voucher model)
    {
        var voucher = await _db.Vouchers.FindAsync(id);
        if (voucher == null) return NotFound();

        voucher.Description = model.Description;
        voucher.DiscountType = model.DiscountType;
        voucher.DiscountValue = model.DiscountValue;
        voucher.MinOrder = model.MinOrder;
        voucher.ExpiryDate = model.ExpiryDate;
        voucher.IsActive = model.IsActive;
        voucher.UsageLimit = model.UsageLimit;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật mã giảm giá.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var voucher = await _db.Vouchers.FindAsync(id);
        if (voucher == null) return NotFound();

        _db.Vouchers.Remove(voucher);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa mã giảm giá.";
        return RedirectToAction(nameof(Index));
    }
}
