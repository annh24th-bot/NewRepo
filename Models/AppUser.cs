using System.ComponentModel.DataAnnotations;

namespace DjStoreWeb.Models;

public static class UserRoles
{
    public const string Admin = "admin";
    public const string Employee = "employee";
    public const string Customer = "customer";
}

public class AppUser
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Role { get; set; } = UserRoles.Customer;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Avatar { get; set; } = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=200&auto=format&fit=crop";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
