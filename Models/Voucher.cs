using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DjStoreWeb.Models;

public static class VoucherDiscountType
{
    public const string Percent = "percent";
    public const string Fixed = "fixed";
}

public class Voucher
{
    [Key]
    [MaxLength(60)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string DiscountType { get; set; } = VoucherDiscountType.Percent;
    public decimal DiscountValue { get; set; }
    public decimal MinOrder { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int UsageLimit { get; set; } = 0; // 0 = unlimited

    public int UsedCount { get; set; } = 0;

    public decimal CalculateDiscount(decimal subtotal)
    {
        if (subtotal < MinOrder) return 0;
        if (DiscountType == VoucherDiscountType.Percent)
            return Math.Round(subtotal * DiscountValue / 100m, 0);
        return DiscountValue;
    }
}
