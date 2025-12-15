namespace CakeShopApi.Models;
public class Category {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public ICollection<Cake>? Cakes { get; set; }
}
