using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using ConstructionStockAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ConstructionStockDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(ConstructionStockDbContext db, TokenService tokenService)
    {
        _db           = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _db.Users
            .Include(u => u.Site)
            .FirstOrDefaultAsync(u => u.Username == request.Username
                                   && u.IsActive  == true);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password." });

        var token = _tokenService.GenerateToken(user, user.Site.SiteName);

        return Ok(new LoginResponseDto
        {
            Token    = token,
            FullName = user.FullName,
            Role     = user.Role,
            SiteId   = user.SiteId,
            UserId   = user.UserId,
            SiteName = user.Site.SiteName
        });
    }

    [HttpGet("hashpassword/{password}")]
    public IActionResult HashPassword(string password)
    {
        return Ok(new { hash = BCrypt.Net.BCrypt.HashPassword(password) });
    }
}
