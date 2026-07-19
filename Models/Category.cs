using System.ComponentModel.DataAnnotations;

namespace DjStoreWeb.Models;

public class Category
{
    [Key]
    [MaxLength(80)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Icon { get; set; } = "Package";

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    public List<Product> Products { get; set; } = new();
}
