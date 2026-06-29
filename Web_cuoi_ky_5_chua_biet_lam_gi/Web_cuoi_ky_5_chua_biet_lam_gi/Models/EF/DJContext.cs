using Core.Database.Models;
using Microsoft.EntityFrameworkCore;


namespace Web_cuoi_ky_5_chua_biet_lam_gi.Models.EF
{
    public class DJContext: DbContext
    {
        public DJContext(DbContextOptions<DJContext> options): base (options) { }
        public DbSet<Articles> Articles { get; set; }
        public DbSet<Authorized> Authorizeds { get; set; }
        public DbSet<Category> Caterories { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
