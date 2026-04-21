using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConstructionStockAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace ConstructionStockAPI.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, string siteName)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("SiteId",                  user.SiteId.ToString()),
            new Claim("SiteName",                siteName),
            new Claim("FullName",                user.FullName)
        };

        var key  = new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(
                                    double.Parse(_config["Jwt:ExpiryHours"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
