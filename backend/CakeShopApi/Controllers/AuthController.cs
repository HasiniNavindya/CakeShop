using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CakeShopApi.Data;
using CakeShopApi.DTOs;
using CakeShopApi.Models;
using CakeShopApi.Services;

namespace CakeShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly ITokenService _token;
    public AuthController(AppDbContext db, ITokenService token) { _db = db; _token = token; }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto) {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email)) return BadRequest("Email already registered");
        var user = new User {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Customer"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var token = _token.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Role, user.Email, user.Name));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return Unauthorized("Invalid credentials");
        var token = _token.CreateToken(user);
        return Ok(new AuthResponseDto(token, user.Role, user.Email, user.Name));
    }
}
