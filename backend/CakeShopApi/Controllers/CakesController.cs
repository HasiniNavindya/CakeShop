using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CakeShopApi.Data;
using CakeShopApi.DTOs;
using CakeShopApi.Models;

namespace CakeShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CakesController : ControllerBase {
    private readonly AppDbContext _db;
    public CakesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll(string? q = null, int? categoryId = null) {
        var query = _db.Cakes.Include(c => c.Category).AsQueryable();
        if (!string.IsNullOrEmpty(q)) query = query.Where(c => c.Name.Contains(q) || (c.Description ?? "").Contains(q));
        if (categoryId.HasValue) query = query.Where(c => c.CategoryId == categoryId.Value);
        var list = await query.Where(c => c.IsActive).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) {
        var cake = await _db.Cakes.Include(c => c.Category).SingleOrDefaultAsync(c => c.Id == id);
        if (cake == null) return NotFound();
        return Ok(cake);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CakeCreateDto dto) {
        var cake = new Cake {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl
        };
        _db.Cakes.Add(cake);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = cake.Id }, cake);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CakeUpdateDto dto) {
        var cake = await _db.Cakes.FindAsync(id);
        if (cake == null) return NotFound();
        if (dto.Name is not null) cake.Name = dto.Name;
        if (dto.Description is not null) cake.Description = dto.Description;
        if (dto.Price.HasValue) cake.Price = dto.Price.Value;
        if (dto.CategoryId.HasValue) cake.CategoryId = dto.CategoryId;
        if (dto.StockQuantity.HasValue) cake.StockQuantity = dto.StockQuantity.Value;
        if (dto.ImageUrl is not null) cake.ImageUrl = dto.ImageUrl;
        if (dto.IsActive.HasValue) cake.IsActive = dto.IsActive.Value;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) {
        var cake = await _db.Cakes.FindAsync(id);
        if (cake == null) return NotFound();
        _db.Cakes.Remove(cake);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
