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
public class TransactionsController : ControllerBase
{
    private readonly ConstructionStockDbContext _db;

    public TransactionsController(ConstructionStockDbContext db)
    {
        _db = db;
    }

    // PUT /api/transactions/{id}/approve
    // StockManager approves a previously recorded IN transaction
    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "StockManager")]
    public async Task<IActionResult> ApproveTransaction(int id)
    {
        var siteId = GetSiteId();
        var approverId = GetUserId();

        var tx = await _db.StockTransactions
            .Include(t => t.Item)
            .FirstOrDefaultAsync(t => t.TransactionId == id && t.SiteId == siteId);

        if (tx == null)
            return NotFound(ApiResponse<object>.Fail("Transaction not found."));

        if (tx.TransactionType != "IN")
            return BadRequest(ApiResponse<object>.Fail("Only IN transactions can be approved."));

        if (tx.IsApproved)
            return BadRequest(ApiResponse<object>.Fail("Transaction is already approved."));

        tx.IsApproved = true;
        tx.ApprovedByUserId = approverId;
        tx.ApprovedAt = DateTime.Now;

        // Persist approval — triggers or application logic should handle adjusting item quantities
        await _db.SaveChangesAsync();

        // Manual stock update since the trigger only fires on INSERT
        if (tx.TransactionType == "IN")
        {
            tx.Item.CurrentQuantity += tx.Quantity;
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<object>.Ok(new { transactionId = tx.TransactionId }, "Transaction approved."));
    }

    // ?? helpers ??????????????????????????????????????????????
    private int GetUserId()  => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int GetSiteId()  => int.Parse(User.FindFirstValue("SiteId")!);
    private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

    // ?? GET /api/transactions/stock-status ???????????????????
    // Stock Manager sees all items on their site
    [HttpGet("stock-status")]
    [Authorize(Roles = "StockManager")]
    public async Task<IActionResult> GetStockStatus()
    {
        var siteId = GetSiteId();

        var items = await _db.Items
            .Where(i => i.SiteId == siteId && i.IsActive == true)
            .Select(i => new StockStatusDto
            {
                ItemId          = i.ItemId,
                ItemName        = i.ItemName,
                Unit            = i.Unit,
                CurrentQuantity = i.CurrentQuantity,
                MinimumQuantity = i.MinimumQuantity,
                StockStatus     = i.CurrentQuantity <= i.MinimumQuantity ? "LOW" : "OK"
            })
            .OrderBy(i => i.ItemName)
            .ToListAsync();

        return Ok(ApiResponse<List<StockStatusDto>>.Ok(items));
    }

    // ?? POST /api/transactions/record ????????????????????????
    // Both roles can record – storekeeper scoped to their site
    [HttpPost("record")]
    [Authorize(Roles = "StockManager,Storekeeper")]
    public async Task<IActionResult> RecordTransaction([FromBody] RecordTransactionDto dto)
    {
        var userId = GetUserId();
        var siteId = GetSiteId();

        // Validate transaction type
        if (dto.TransactionType != "IN" && dto.TransactionType != "OUT")
            return BadRequest(ApiResponse<object>.Fail("TransactionType must be IN or OUT."));

        // Supplier required for IN
        if (dto.TransactionType == "IN" && dto.SupplierId == null)
            return BadRequest(ApiResponse<object>.Fail("SupplierId is required for Stock IN."));

        // Fetch item and verify it belongs to this site
        var item = await _db.Items
            .FirstOrDefaultAsync(i => i.ItemId == dto.ItemId
                                   && i.SiteId  == siteId
                                   && i.IsActive == true);

        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Item not found on your site."));

        // Prevent negative stock
        if (dto.TransactionType == "OUT" && dto.Quantity > item.CurrentQuantity)
            return BadRequest(ApiResponse<object>.Fail($"Insufficient stock. Current quantity is {item.CurrentQuantity}."));

        var transaction = new StockTransaction
        {
            ItemId           = dto.ItemId,
            SiteId           = siteId,
            RecordedByUserId = userId,
            SupplierId       = dto.TransactionType == "IN" ? dto.SupplierId : null,
            TransactionType  = dto.TransactionType,
            Quantity         = dto.Quantity,
            Remarks          = dto.Remarks,
            TransactionDate  = DateTime.Now,
            IsApproved       = dto.TransactionType == "IN" && GetRole() == "StockManager" ? true : false,
            ApprovedByUserId = dto.TransactionType == "IN" && GetRole() == "StockManager" ? userId : null,
            ApprovedAt       = dto.TransactionType == "IN" && GetRole() == "StockManager" ? DateTime.Now : null
        };

        _db.StockTransactions.Add(transaction);
        
        // Only update CurrentQuantity immediately if it's an OUT transaction 
        // OR an immediately approved IN transaction (recorded by Manager)
        if (transaction.TransactionType == "OUT")
        {
            item.CurrentQuantity -= transaction.Quantity;
        }
        else if (transaction.TransactionType == "IN" && transaction.IsApproved)
        {
            item.CurrentQuantity += transaction.Quantity;
        }

        await _db.SaveChangesAsync(); // trigger fires here – raises alert if needed

        return Ok(ApiResponse<object>.Ok(
            new { transactionId = transaction.TransactionId },
            $"Stock {dto.TransactionType} recorded successfully."));
    }

    // ?? GET /api/transactions/log ?????????????????????????????
    // Stock Manager sees full site log
    // Storekeeper sees only their own records
    [HttpGet("log")]
    [Authorize(Roles = "StockManager,Storekeeper")]
    public async Task<IActionResult> GetTransactionLog()
    {
        var userId = GetUserId();
        var siteId = GetSiteId();
        var role   = GetRole();

        var query = _db.StockTransactions
            .Include(t => t.Item)
            .Include(t => t.RecordedByUser)
            .Include(t => t.Supplier)
            .Include(t => t.ApprovedByUser)
            .Where(t => t.SiteId == siteId)
            .AsQueryable();

        // Storekeeper only sees their own records
        if (role == "Storekeeper")
            query = query.Where(t => t.RecordedByUserId == userId);

        var log = await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionResponseDto
            {
                TransactionId   = t.TransactionId,
                ItemName        = t.Item.ItemName,
                TransactionType = t.TransactionType,
                Quantity        = t.Quantity,
                SupplierName    = t.Supplier != null ? t.Supplier.SupplierName : null,
                RecordedBy      = t.RecordedByUser.FullName,
                Remarks         = t.Remarks,
                IsApproved      = t.IsApproved,
                ApprovedBy      = t.ApprovedByUserId != null ? _db.Users.FirstOrDefault(u => u.UserId == t.ApprovedByUserId)!.FullName : null,
                ApprovedAt      = t.ApprovedAt,
                TransactionDate = t.TransactionDate
            })
            .ToListAsync();

        return Ok(ApiResponse<List<TransactionResponseDto>>.Ok(log));
    }

    // ?? GET /api/transactions/items ???????????????????????????
    // Returns items for the dropdown when recording a transaction
    [HttpGet("items")]
    [Authorize(Roles = "StockManager,Storekeeper")]
    public async Task<IActionResult> GetItems()
    {
        var siteId = GetSiteId();

        var items = await _db.Items
            .Where(i => i.SiteId == siteId && i.IsActive == true)
            .Select(i => new { i.ItemId, i.ItemName, i.Unit, i.CurrentQuantity })
            .OrderBy(i => i.ItemName)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    // ?? GET /api/transactions/suppliers ??????????????????????
    // Returns active suppliers for the IN transaction dropdown
    [HttpGet("suppliers")]
    [Authorize(Roles = "StockManager,Storekeeper")]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _db.Suppliers
            .Where(s => s.IsActive == true)
            .Select(s => new { s.SupplierId, s.SupplierName, s.ContactPerson, s.Phone })
            .OrderBy(s => s.SupplierName)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(suppliers));
    }
}
