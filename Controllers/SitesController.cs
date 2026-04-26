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
[Authorize(Roles = "Admin")]
public class SitesController : ControllerBase
{
    private readonly ConstructionStockDbContext _db;

    public SitesController(ConstructionStockDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSites()
    {
        var sites = await _db.Sites
            .OrderBy(s => s.SiteName)
            .Select(s => new SiteResponseDto
            {
                SiteId = s.SiteId,
                SiteName = s.SiteName,
                Location = s.Location,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SiteResponseDto>>.Ok(sites));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite([FromBody] CreateSiteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SiteName))
            return BadRequest(ApiResponse<object>.Fail("Site name is required."));

        var exists = await _db.Sites.AnyAsync(s => s.SiteName.ToLower() == dto.SiteName.Trim().ToLower());
        if (exists)
            return BadRequest(ApiResponse<object>.Fail("A site with this name already exists."));

        var site = new Site
        {
            SiteName = dto.SiteName.Trim(),
            Location = dto.Location?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _db.Sites.Add(site);
        await _db.SaveChangesAsync();

        // Assign users if provided
        if (dto.AssignUserIds != null && dto.AssignUserIds.Length > 0)
        {
            var users = await _db.Users.Where(u => dto.AssignUserIds.Contains(u.UserId)).ToListAsync();
            foreach (var user in users)
            {
                user.SiteId = site.SiteId;
            }
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<object>.Ok(new { siteId = site.SiteId }, "Site created."));
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("User not found."));

        user.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { userId = user.UserId }, "User status updated."));
    }

    [HttpPut("users/{id:int}/site")]
    public async Task<IActionResult> UpdateUserSite(int id, [FromBody] UpdateUserSiteDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("User not found."));

        var site = await _db.Sites.FindAsync(dto.SiteId);
        if (site == null)
            return NotFound(ApiResponse<object>.Fail("Site not found."));

        user.SiteId = dto.SiteId;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { userId = user.UserId }, "User site updated."));
    }
}
