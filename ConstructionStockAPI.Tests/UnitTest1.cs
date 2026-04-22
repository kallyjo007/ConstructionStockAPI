using System.Security.Claims;
using ConstructionStockAPI.Controllers;
using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using ConstructionStockAPI.Helpers;
using ConstructionStockAPI.Models;
using ConstructionStockAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConstructionStockAPI.Tests;

public class ApiHappyPathTests
{
    [Fact]
    public async Task Login_ReturnsOk_WithToken()
    {
        using var db = CreateDbContext();

        var site = new Site
        {
            SiteId = 1,
            SiteName = "Site A",
            Location = "Kigali",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        db.Sites.Add(site);
        db.Users.Add(new User
        {
            UserId = 10,
            SiteId = site.SiteId,
            Site = site,
            FullName = "Manager One",
            Username = "manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = "StockManager",
            IsActive = true,
            CreatedAt = DateTime.Now
        });

        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super_secret_key_for_tests_only_1234567890",
                ["Jwt:Issuer"] = "ConstructionStockAPI",
                ["Jwt:Audience"] = "ConstructionStockClient",
                ["Jwt:ExpiryHours"] = "8"
            })
            .Build();

        var controller = new AuthController(db, new TokenService(config));
        var result = await controller.Login(new LoginRequestDto
        {
            Username = "manager",
            Password = "Password123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(ok.Value);

        Assert.True(response.Success);
        Assert.Equal("Login successful.", response.Message);
        Assert.False(string.IsNullOrWhiteSpace(response.Data?.Token));
    }

    [Fact]
    public async Task GetItems_ReturnsOnlyCurrentSiteItems()
    {
        using var db = CreateDbContext();
        db.Items.AddRange(
            new Item
            {
                ItemId = 1,
                SiteId = 1,
                ItemName = "Cement",
                Unit = "Bags",
                MinimumQuantity = 10,
                CurrentQuantity = 20,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Item
            {
                ItemId = 2,
                SiteId = 2,
                ItemName = "Steel Rods",
                Unit = "Pieces",
                MinimumQuantity = 5,
                CurrentQuantity = 3,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        await db.SaveChangesAsync();

        var controller = new ItemsController(db);
        controller.ControllerContext = BuildControllerContext(1, 100, "StockManager");

        var result = await controller.GetItems();
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<List<ItemResponseDto>>>(ok.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data!);
        Assert.Equal("Cement", response.Data![0].ItemName);
    }

    [Fact]
    public async Task RecordTransaction_In_ReturnsSuccess()
    {
        using var db = CreateDbContext();

        db.Items.Add(new Item
        {
            ItemId = 5,
            SiteId = 1,
            ItemName = "Sand",
            Unit = "Tons",
            MinimumQuantity = 2,
            CurrentQuantity = 15,
            IsActive = true,
            CreatedAt = DateTime.Now
        });

        db.Suppliers.Add(new Supplier
        {
            SupplierId = 7,
            SupplierName = "ABC Supply",
            IsActive = true,
            CreatedAt = DateTime.Now
        });

        await db.SaveChangesAsync();

        var controller = new TransactionsController(db);
        controller.ControllerContext = BuildControllerContext(1, 42, "Storekeeper");

        var result = await controller.RecordTransaction(new RecordTransactionDto
        {
            ItemId = 5,
            Quantity = 4,
            TransactionType = "IN",
            SupplierId = 7,
            Remarks = "Test delivery"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);

        Assert.True(response.Success);
        Assert.Contains("recorded successfully", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await db.StockTransactions.CountAsync());
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
