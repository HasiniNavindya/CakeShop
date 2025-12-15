using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using CakeShopApi.Data;
using CakeShopApi.DTOs;
using CakeShopApi.Models;

namespace CakeShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase {
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) { _db = db; }

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CreateOrderDto dto) {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        var userId = int.Parse(userIdClaim);

        var cakeIds = dto.Items.Select(i => i.CakeId).Distinct().ToList();
        var cakes = await _db.Cakes.Where(c => cakeIds.Contains(c.Id)).ToListAsync();

        foreach (var item in dto.Items) {
            var cake = cakes.SingleOrDefault(c => c.Id == item.CakeId);
            if (cake == null) return BadRequest($"Cake id {item.CakeId} not found");
            if (cake.StockQuantity < item.Quantity) return BadRequest($"Not enough stock for {cake.Name}");
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try {
            var order = new Order {
                UserId = userId,
                Address = dto.Address,
                Status = "Pending",
                Items = dto.Items.Select(i => {
                    var c = cakes.Single(x => x.Id == i.CakeId);
                    var price = c.Price;
                    return new OrderItem {
                        CakeId = i.CakeId,
                        Quantity = i.Quantity,
                        Price = price,
                        SubTotal = price * i.Quantity
                    };
                }).ToList()
            };
            order.Total = order.Items!.Sum(it => it.SubTotal);
            _db.Orders.Add(order);

            foreach (var it in order.Items!) {
                var cake = cakes.Single(c => c.Id == it.CakeId);
                cake.StockQuantity -= it.Quantity;
                _db.Cakes.Update(cake);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        } catch {
            await tx.RollbackAsync();
            throw;
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyOrders() {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        var userId = int.Parse(userIdClaim);
        var orders = await _db.Orders.Include(o => o.Items!).ThenInclude(i => i.Cake).Where(o => o.UserId == userId).ToListAsync();
        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders() {
        var orders = await _db.Orders.Include(o => o.Items!).ThenInclude(i => i.Cake).Include(o => o.User).ToListAsync();
        return Ok(orders);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id) {
        var order = await _db.Orders.Include(o => o.Items!).ThenInclude(i => i.Cake).SingleOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var userId = userIdClaim != null ? int.Parse(userIdClaim) : -1;
        if (User.IsInRole("Admin") == false && order.UserId != userId) return Forbid();
        return Ok(order);
    }
}
