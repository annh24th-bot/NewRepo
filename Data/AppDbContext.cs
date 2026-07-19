using DjStoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DjStoreWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();
    public DbSet<InventoryLog> InventoryLogs => Set<InventoryLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderCode)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQLite has no native "decimal" type — by default EF Core stores decimal as TEXT,
        // which breaks ORDER BY / SUM / comparisons in SQL (SQLite would sort/compare them
        // as strings). Storing money values as REAL (double) instead lets all of these
        // operations translate correctly to SQL. Precision loss is not a concern here since
        // product prices are whole VND amounts well within double's exact integer range.
        var decimalConverter = new ValueConverter<decimal, double>(
            v => (double)v,
            v => (decimal)v);

        var nullableDecimalConverter = new ValueConverter<decimal?, double?>(
            v => v.HasValue ? (double?)(double)v.Value : null,
            v => v.HasValue ? (decimal?)(decimal)v.Value : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal))
                {
                    property.SetValueConverter(decimalConverter);
                }
                else if (property.ClrType == typeof(decimal?))
                {
                    property.SetValueConverter(nullableDecimalConverter);
                }
            }
        }
    }
}
