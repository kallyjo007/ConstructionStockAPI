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
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly ConstructionStockDbContext _db;

    public ItemsController(ConstructionStockDbContext db)
    {
        _db = db;
    }

    private int GetSiteId() => int.Parse(User.FindFirstValue("SiteId")!);

    [HttpGet]
    [Authorize(Roles = "StockManager,Storekeeper")]
    public async Task<IActionResult> GetItems()
    {
        var siteId = GetSiteId();

        var items = await _db.Items
            .Where(i => i.SiteId == siteId && i.IsActive == true)
            .Select(i => new ItemResponseDto
            {
                ItemId = i.ItemId,
                ItemName = i.ItemName,
                Unit = i.Unit,
                MinimumQuantity = i.MinimumQuantity,
                CurrentQuantity = i.CurrentQuantity,
                StockStatus = i.CurrentQuantity <= i.MinimumQuantity ? "LOW" : "OK"
            })
            .OrderBy(i => i.ItemName)
            .ToListAsync();

        return Ok(ApiResponse<List<ItemResponseDto>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "StockManager")]
    public async Task<IActionResult> GetItemById(int id)
    {
        var siteId = GetSiteId();

        var item = await _db.Items
            .Where(i => i.ItemId == id && i.SiteId == siteId && i.IsActive == true)
            .Select(i => new ItemResponseDto
            {
                ItemId = i.ItemId,
                ItemName = i.ItemName,
                Unit = i.Unit,
                MinimumQuantity = i.MinimumQuantity,
                CurrentQuantity = i.CurrentQuantity,
                StockStatus = i.CurrentQuantity <= i.MinimumQuantity ? "LOW" : "OK"
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Item not found on your site."));

        return Ok(ApiResponse<ItemResponseDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Roles = "StockManager")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
    {
        var siteId = GetSiteId();
        var normalizedName = dto.ItemName.Trim();

        var exists = await _db.Items.AnyAsync(i =>
            i.SiteId == siteId &&
            i.IsActive == true &&
            i.ItemName.ToLower() == normalizedName.ToLower());

        if (exists)
            return BadRequest(ApiResponse<object>.Fail("An active item with this name already exists on your site."));

        var item = new Item
        {
            SiteId = siteId,
            ItemName = normalizedName,
            Unit = dto.Unit.Trim(),
            MinimumQuantity = dto.MinimumQuantity,
            CurrentQuantity = dto.CurrentQuantity,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        var response = new ItemResponseDto
        {
            ItemId = item.ItemId,
            ItemName = item.ItemName,
            Unit = item.Unit,
            MinimumQuantity = item.MinimumQuantity,
            CurrentQuantity = item.CurrentQuantity,
            StockStatus = item.CurrentQuantity <= item.MinimumQuantity ? "LOW" : "OK"
        };

        return CreatedAtAction(
            nameof(GetItemById),
            new { id = item.ItemId },
            ApiResponse<ItemResponseDto>.Ok(response, "Item created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "StockManager")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto dto)
    {
        var siteId = GetSiteId();
        var normalizedName = dto.ItemName.Trim();

        var item = await _db.Items
            .FirstOrDefaultAsync(i => i.ItemId == id && i.SiteId == siteId && i.IsActive == true);

        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Item not found on your site."));

        var duplicateNameExists = await _db.Items.AnyAsync(i =>
            i.ItemId != id &&
            i.SiteId == siteId &&
            i.IsActive == true &&
            i.ItemName.ToLower() == normalizedName.ToLower());

        if (duplicateNameExists)
            return BadRequest(ApiResponse<object>.Fail("Another active item with this name already exists on your site."));

        item.ItemName = normalizedName;
        item.Unit = dto.Unit.Trim();
        item.MinimumQuantity = dto.MinimumQuantity;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { itemId = item.ItemId }, "Item updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "StockManager")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var siteId = GetSiteId();

        var item = await _db.Items
            .FirstOrDefaultAsync(i => i.ItemId == id && i.SiteId == siteId && i.IsActive == true);

        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Item not found on your site."));

        item.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { itemId = item.ItemId }, "Item deleted successfully."));
    }
}
