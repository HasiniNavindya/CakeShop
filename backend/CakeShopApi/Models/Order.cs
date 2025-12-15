namespace CakeShopApi.Models;
public class Order {
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem>? Items { get; set; }
}
