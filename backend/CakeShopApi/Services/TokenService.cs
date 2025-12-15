using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using CakeShopApi.Models;

namespace CakeShopApi.Services;
public class TokenService : ITokenService {
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) { _config = config; }

    public string CreateToken(User user) {
        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new Exception("JWT Key missing");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var durationMin = int.Parse(jwtSection["DurationMinutes"] ?? "60");

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("name", user.Name)
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(durationMin), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
