using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CakeShopApi.Data;
using CakeShopApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace CakeShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    // GET: api/Categories
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories.ToListAsync();
        return Ok(categories);
    }

    // POST: api/Categories (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(category);
    }
}
