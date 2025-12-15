namespace CakeShopApi.Models;
public class User {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Customer";
    public ICollection<Order>? Orders { get; set; }
}
