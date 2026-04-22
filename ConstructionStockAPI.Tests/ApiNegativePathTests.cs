using System.Security.Claims;
using ConstructionStockAPI.Controllers;
using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using ConstructionStockAPI.Helpers;
using ConstructionStockAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConstructionStockAPI.Tests;

public class ApiNegativePathTests
{
    [Fact]
    public async Task RecordTransaction_OutExceedsStock_ReturnsBadRequest()
    {
        using var db = CreateDbContext();
        db.Items.Add(new Item
        {
            ItemId = 1,
            SiteId = 1,
            ItemName = "Cement",
            Unit = "Bags",
            MinimumQuantity = 10,
            CurrentQuantity = 5, // Low stock
            IsActive = true,
            CreatedAt = DateTime.Now
        });
        await db.SaveChangesAsync();

        var controller = new TransactionsController(db);
        controller.ControllerContext = BuildControllerContext(1, 42, "Storekeeper");

        var result = await controller.RecordTransaction(new RecordTransactionDto
        {
            ItemId = 1,
            Quantity = 10, // More than current (5)
            TransactionType = "OUT"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);

        Assert.False(response.Success);
        Assert.Contains("Insufficient stock", response.Message);
    }

    [Fact]
    public async Task RecordTransaction_InWithoutSupplier_ReturnsBadRequest()
    {
        using var db = CreateDbContext();
        var controller = new TransactionsController(db);
        controller.ControllerContext = BuildControllerContext(1, 42, "Storekeeper");

        var result = await controller.RecordTransaction(new RecordTransactionDto
        {
            ItemId = 1,
            Quantity = 10,
            TransactionType = "IN",
            SupplierId = null // Missing
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);

        Assert.False(response.Success);
        Assert.Contains("SupplierId is required", response.Message);
    }

    [Fact]
    public async Task GetItemById_FromAnotherSite_ReturnsNotFound()
    {
        using var db = CreateDbContext();
        db.Items.Add(new Item
        {
            ItemId = 10,
            SiteId = 2, // Site 2
            ItemName = "Secret Item",
            Unit = "Pieces",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var controller = new ItemsController(db);
        controller.ControllerContext = BuildControllerContext(1, 42, "StockManager"); // Site 1

        var result = await controller.GetItemById(10);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);

        Assert.False(response.Success);
        Assert.Contains("Item not found on your site", response.Message);
    }

    [Fact]
    public async Task CreateItem_DuplicateName_ReturnsBadRequest()
    {
        using var db = CreateDbContext();
        db.Items.Add(new Item
        {
            ItemId = 1,
            SiteId = 1,
            ItemName = "Cement",
            Unit = "Bags",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var controller = new ItemsController(db);
        controller.ControllerContext = BuildControllerContext(1, 42, "StockManager");

        var result = await controller.CreateItem(new CreateItemDto
        {
            ItemName = "  CEMENT  ", // Test case insensitivity and trimming
            Unit = "Bags",
            CurrentQuantity = 10,
            MinimumQuantity = 5
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);

        Assert.False(response.Success);
        Assert.Contains("already exists", response.Message);
    }

    private static ConstructionStockDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ConstructionStockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ConstructionStockDbContext(options);
    }

    private static ControllerContext BuildControllerContext(int siteId, int userId, string role)
    {
        var claims = new[]
        {
            new Claim("SiteId", siteId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
