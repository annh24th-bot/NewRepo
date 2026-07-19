using System.Text.Json;
using DjStoreWeb.Models;
using DjStoreWeb.Services;

namespace DjStoreWeb.Data.Seed;

public static class SeedRunner
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task RunAsync(AppDbContext db, string contentRootPath)
    {
        await db.Database.EnsureCreatedAsync();

        string seedFolder = Path.Combine(contentRootPath, "Data", "Seed");

        if (!db.Categories.Any())
        {
            var categories = await ReadJsonAsync<List<CategorySeedDto>>(Path.Combine(seedFolder, "categories.json"));
            if (categories != null)
            {
                foreach (var c in categories)
                {
                    db.Categories.Add(new Category
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Icon = c.Icon,
                        Description = c.Description
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        if (!db.Products.Any())
        {
            var products = await ReadJsonAsync<List<ProductSeedDto>>(Path.Combine(seedFolder, "products.json"));
            var validCategoryIds = db.Categories.Select(c => c.Id).ToHashSet();
            var rnd = new Random(42);

            var digitalCategoryIds = new HashSet<string> { "music-pack", "software-license", "google-drive" };

            if (products != null)
            {
                foreach (var p in products)
                {
                    string categoryId = validCategoryIds.Contains(p.Category) ? p.Category : validCategoryIds.FirstOrDefault() ?? "accessories";

                    db.Products.Add(new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryId = categoryId,
                        Brand = p.Brand,
                        Price = p.Price,
                        OriginalPrice = p.OriginalPrice,
                        Rating = p.Rating,
                        Reviews = p.Reviews,
                        Image = p.Image,
                        Description = p.Description,
                        SpecsJson = JsonSerializer.Serialize(p.Specs ?? new Dictionary<string, string>()),
                        FlashSale = p.FlashSale,
                        Discount = p.Discount,
                        Stock = rnd.Next(5, 60),
                        IsActive = true,
                        DriveLink = digitalCategoryIds.Contains(categoryId)
                            ? $"https://drive.google.com/drive/folders/demo-{p.Id}"
                            : null
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        if (!db.Users.Any())
        {
            var users = await ReadJsonAsync<List<UserSeedDto>>(Path.Combine(seedFolder, "users.json"));
            if (users != null)
            {
                foreach (var u in users)
                {
                    db.Users.Add(new AppUser
                    {
                        Email = u.Email.Trim().ToLowerInvariant(),
                        PasswordHash = PasswordHasher.Hash(u.Password),
                        Role = string.IsNullOrWhiteSpace(u.Role) ? UserRoles.Customer : u.Role,
                        Name = u.Name,
                        Phone = u.Phone,
                        Address = u.Address,
                        Avatar = string.IsNullOrWhiteSpace(u.Avatar)
                            ? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=200&auto=format&fit=crop"
                            : u.Avatar
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        if (!db.Orders.Any())
        {
            var orders = await ReadJsonAsync<List<OrderSeedDto>>(Path.Combine(seedFolder, "orders.json"));
            if (orders != null)
            {
                foreach (var o in orders)
                {
                    var order = new Order
                    {
                        OrderCode = o.OrderId,
                        CustomerEmail = o.CustomerEmail.Trim().ToLowerInvariant(),
                        CustomerName = o.CustomerName,
                        Date = o.Date == default ? DateTime.UtcNow : o.Date,
                        Subtotal = o.Subtotal,
                        Discount = o.Discount,
                        VoucherCode = string.IsNullOrWhiteSpace(o.VoucherCode) ? null : o.VoucherCode,
                        Total = o.Total,
                        Status = string.IsNullOrWhiteSpace(o.Status) ? OrderStatus.Pending : o.Status,
                        PaymentMethod = o.PaymentMethod,
                        ShippingAddress = o.ShippingAddress,
                        Phone = o.Phone
                    };
                    foreach (var it in o.Items)
                    {
                        order.Items.Add(new OrderItem
                        {
                            ProductId = it.Id,
                            Name = it.Name,
                            Price = it.Price,
                            Quantity = it.Quantity,
                            Image = it.Image
                        });
                    }
                    db.Orders.Add(order);
                }
                await db.SaveChangesAsync();
            }
        }

        if (!db.Vouchers.Any())
        {
            db.Vouchers.AddRange(
                new Voucher { Code = "HELLOSUMMER", Description = "Giảm 500.000đ cho đơn từ 5.000.000đ", DiscountType = VoucherDiscountType.Fixed, DiscountValue = 500000, MinOrder = 5000000, IsActive = true, ExpiryDate = DateTime.UtcNow.AddMonths(3) },
                new Voucher { Code = "DJNEWBIE10", Description = "Giảm 10% cho khách hàng mới, tối đa đơn 20.000.000đ", DiscountType = VoucherDiscountType.Percent, DiscountValue = 10, MinOrder = 0, IsActive = true, ExpiryDate = DateTime.UtcNow.AddMonths(6) },
                new Voucher { Code = "FREESHIP", Description = "Miễn phí vận chuyển toàn quốc", DiscountType = VoucherDiscountType.Fixed, DiscountValue = 50000, MinOrder = 1000000, IsActive = true, ExpiryDate = DateTime.UtcNow.AddMonths(1) }
            );
            await db.SaveChangesAsync();
        }

        if (!db.NewsPosts.Any())
        {
            db.NewsPosts.AddRange(
                new NewsPost
                {
                    Title = "Hướng dẫn chọn mua DJ Controller cho người mới bắt đầu",
                    Category = "Cẩm nang",
                    Author = "DJ Store",
                    Image = "https://images.unsplash.com/photo-1598488035139-bdbb2231ce04?q=80&w=800",
                    Content = "Việc lựa chọn một DJ Controller phù hợp là bước đầu tiên quan trọng trên con đường trở thành DJ chuyên nghiệp. Bài viết này chia sẻ các tiêu chí về số kênh điều khiển, khả năng tương thích phần mềm và ngân sách phù hợp cho người mới.",
                    Views = 128,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new NewsPost
                {
                    Title = "Rekordbox vs Serato DJ: Nên chọn phần mềm nào năm 2026?",
                    Category = "Phần mềm",
                    Author = "DJ Store",
                    Image = "https://images.unsplash.com/photo-1516873240891-4bf014598ab4?q=80&w=800",
                    Content = "So sánh chi tiết giữa hai nền tảng phần mềm DJ phổ biến nhất hiện nay về giá bản quyền, thư viện hiệu ứng, khả năng đồng bộ đám mây và trải nghiệm người dùng thực tế trên sân khấu.",
                    Views = 96,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new NewsPost
                {
                    Title = "Khuyến mãi mùa hè: Giảm giá sốc thiết bị DJ chính hãng",
                    Category = "Khuyến mãi",
                    Author = "DJ Store",
                    Image = "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?q=80&w=800",
                    Content = "Chương trình Flash Sale mùa hè chính thức khởi động với hàng loạt ưu đãi hấp dẫn dành cho các dòng sản phẩm mixer, tai nghe kiểm âm và loa monitor phòng thu.",
                    Views = 210,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            );
            await db.SaveChangesAsync();
        }

        if (!db.Reviews.Any())
        {
            var firstProductIds = db.Products.Select(p => p.Id).Take(3).ToList();
            if (firstProductIds.Count > 0)
            {
                db.Reviews.Add(new Review
                {
                    ProductId = firstProductIds[0],
                    CustomerName = "Minh Tuấn",
                    CustomerEmail = "customer@djstore.com",
                    Rating = 5,
                    Comment = "Sản phẩm chất lượng, đóng gói cẩn thận, giao hàng nhanh. Rất đáng tiền!",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    AdminReply = "Cảm ơn anh đã tin tưởng DJ Store! Chúc anh có những buổi trình diễn thật cháy 🎧",
                    RepliedAt = DateTime.UtcNow.AddDays(-5)
                });
            }
            if (firstProductIds.Count > 1)
            {
                db.Reviews.Add(new Review
                {
                    ProductId = firstProductIds[1],
                    CustomerName = "Thảo Nguyên",
                    CustomerEmail = "thao.nguyen@example.com",
                    Rating = 4,
                    Comment = "Hàng tốt nhưng giao hơi chậm so với dự kiến. Mong shop cải thiện thêm.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                });
            }
            await db.SaveChangesAsync();
        }

        if (!db.ContactMessages.Any())
        {
            db.ContactMessages.Add(new ContactMessage
            {
                Name = "Quang Huy",
                Email = "quanghuy.dj@example.com",
                Message = "Chào shop, mình muốn hỏi bàn DDJ-FLX4 còn hàng không và có ship về Đà Nẵng không ạ?",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                IsHandled = false
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task<T?> ReadJsonAsync<T>(string path)
    {
        if (!File.Exists(path)) return default;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOpts);
    }
}
