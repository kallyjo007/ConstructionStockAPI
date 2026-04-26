using System.Security.Claims;
using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using ConstructionStockAPI.Helpers;
using ConstructionStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,StockManager")]
public class UsersController : ControllerBase
{
    private readonly ConstructionStockDbContext _db;

    public UsersController(ConstructionStockDbContext db)
    {
        _db = db;
    }

    private int GetSiteId() => int.Parse(User.FindFirstValue("SiteId")!);

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var query = _db.Users.Include(u => u.Site).AsQueryable();

        // If not Admin, only see users for their site
        if (role != "Admin")
        {
            var siteId = GetSiteId();
            query = query.Where(u => u.SiteId == siteId);
        }

        var users = await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                Role = u.Role,
                SiteId = u.SiteId,
                SiteName = u.Site.SiteName,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserResponseDto>>.Ok(users));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var query = _db.Users.Include(u => u.Site).AsQueryable();

        if (role != "Admin")
        {
            var siteId = GetSiteId();
            query = query.Where(u => u.UserId == id && u.SiteId == siteId);
        }
        else
        {
            query = query.Where(u => u.UserId == id);
        }

        var user = await query
            .Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                Role = u.Role,
                SiteId = u.SiteId,
                SiteName = u.Site.SiteName,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(ApiResponse<object>.Fail("User not found."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(ApiResponse<object>.Fail("Username and password are required."));

        var exists = await _db.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.Trim().ToLower());
        if (exists)
            return BadRequest(ApiResponse<object>.Fail("Username already exists."));

        var site = await _db.Sites.FindAsync(dto.SiteId);
        if (site == null)
            return NotFound(ApiResponse<object>.Fail("Site not found."));

        var newUser = new User
        {
            FullName = dto.FullName.Trim(),
            Username = dto.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            SiteId = dto.SiteId,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { userId = newUser.UserId }, "User created."));
    }
}
