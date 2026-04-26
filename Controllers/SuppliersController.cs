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
[Authorize(Roles = "StockManager,Admin")]
public class SuppliersController : ControllerBase
{
    private readonly ConstructionStockDbContext _db;

    public SuppliersController(ConstructionStockDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _db.Suppliers
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.SupplierName)
            .Select(s => new SupplierResponseDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SupplierResponseDto>>.Ok(suppliers));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        var supplier = await _db.Suppliers
            .Where(s => s.SupplierId == id && s.IsActive == true)
            .Select(s => new SupplierDetailDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                RecentDeliveries = s.StockTransactions
                    .Where(t => t.TransactionType == "IN")
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(20)
                    .Select(t => new SupplierDeliveryHistoryDto
                    {
                        TransactionId = t.TransactionId,
                        TransactionDate = t.TransactionDate,
                        ItemName = t.Item.ItemName,
                        Quantity = t.Quantity,
                        RecordedBy = t.RecordedByUser.FullName
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (supplier == null)
            return NotFound(ApiResponse<object>.Fail("Supplier not found."));

        return Ok(ApiResponse<SupplierDetailDto>.Ok(supplier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
    {
        var normalizedName = dto.SupplierName.Trim();

        var exists = await _db.Suppliers.AnyAsync(s =>
            s.IsActive == true &&
            s.SupplierName.ToLower() == normalizedName.ToLower());

        if (exists)
            return BadRequest(ApiResponse<object>.Fail("An active supplier with this name already exists."));

        var supplier = new Supplier
        {
            SupplierName = normalizedName,
            ContactPerson = dto.ContactPerson?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address?.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { supplierId = supplier.SupplierId }, "Supplier created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
    {
        var supplier = await _db.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == id && s.IsActive == true);

        if (supplier == null)
            return NotFound(ApiResponse<object>.Fail("Supplier not found."));

        var normalizedName = dto.SupplierName.Trim();
        var duplicate = await _db.Suppliers.AnyAsync(s =>
            s.SupplierId != id &&
            s.IsActive == true &&
            s.SupplierName.ToLower() == normalizedName.ToLower());

        if (duplicate)
            return BadRequest(ApiResponse<object>.Fail("Another active supplier with this name already exists."));

        supplier.SupplierName = normalizedName;
        supplier.ContactPerson = dto.ContactPerson?.Trim();
        supplier.Phone = dto.Phone?.Trim();
        supplier.Email = dto.Email?.Trim();
        supplier.Address = dto.Address?.Trim();
        supplier.Latitude = dto.Latitude;
        supplier.Longitude = dto.Longitude;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { supplierId = supplier.SupplierId }, "Supplier updated successfully."));
    }
}
