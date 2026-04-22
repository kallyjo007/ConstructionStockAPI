using System.Security.Claims;
using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using ConstructionStockAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "StockManager")]
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
        var siteId = GetSiteId();

        var users = await _db.Users
            .Where(u => u.SiteId == siteId)
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
        var siteId = GetSiteId();

        var user = await _db.Users
            .Where(u => u.UserId == id && u.SiteId == siteId)
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
            return NotFound(ApiResponse<object>.Fail("User not found on your site."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }
}
