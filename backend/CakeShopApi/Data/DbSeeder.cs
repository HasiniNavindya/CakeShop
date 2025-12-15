using CakeShopApi.Models;

namespace CakeShopApi.Data;
public static class DbSeeder {
    public static void Seed(AppDbContext db) {
        if (db.Users.Any()) return;

        var admin = new User {
            Name = "Admin",
            Email = "admin@cakeshop.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin"
        };
        db.Users.Add(admin);

        var cat1 = new Category { Name = "Birthday" };
        var cat2 = new Category { Name = "Wedding" };
        db.Categories.AddRange(cat1, cat2);

        db.Cakes.AddRange(
            new Cake { Name = "Chocolate Fudge", Description="Rich chocolate cake", Price = 12.50m, Category = cat1, StockQuantity = 10, ImageUrl = "" },
            new Cake { Name = "Vanilla Dream", Description="Classic vanilla", Price = 9.99m, Category = cat1, StockQuantity = 8 }
        );

        db.SaveChanges();
    }
}
