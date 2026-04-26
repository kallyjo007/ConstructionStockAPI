using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ConstructionStockAPI.Services;

public class ReportService
{
    private readonly ConstructionStockDbContext _db;

    public ReportService(ConstructionStockDbContext db)
    {
        _db = db;
    }

    public async Task<DailyReportDto> GetDailyReportAsync(int siteId, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var transactions = await _db.StockTransactions
            .Where(t => t.SiteId == siteId && t.TransactionDate >= start && t.TransactionDate < end)
            .OrderBy(t => t.TransactionDate)
            .Select(t => new DailyReportTransactionDto
            {
                TransactionId = t.TransactionId,
                TransactionDate = t.TransactionDate,
                ItemName = t.Item.ItemName,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                SupplierName = t.Supplier != null ? t.Supplier.SupplierName : null,
                RecordedBy = t.RecordedByUser.FullName
            })
            .ToListAsync();

        return new DailyReportDto
        {
            Date = date,
            TotalInQuantity = transactions
                .Where(t => t.TransactionType == "IN")
                .Sum(t => t.Quantity),
            TotalOutQuantity = transactions
                .Where(t => t.TransactionType == "OUT")
                .Sum(t => t.Quantity),
            Transactions = transactions
        };
    }

    public async Task<AdminReportDto> GetAdminReportAsync(DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var transactions = await _db.StockTransactions
            .Include(t => t.Site)
            .Include(t => t.RecordedByUser)
            .ThenInclude(u => u.Site)
            .Where(t => t.TransactionDate >= start && t.TransactionDate < end)
            .ToListAsync();

        var sites = transactions
            .GroupBy(t => t.Site)
            .Select(g => new SiteActivityDto
            {
                SiteName = g.Key.SiteName,
                Location = g.Key.Location,
                TransactionCount = g.Count()
            })
            .ToList();

        var users = transactions
            .GroupBy(t => t.RecordedByUser)
            .Select(g => new UserActivityDto
            {
                FullName = g.Key.FullName,
                Role = g.Key.Role,
                SiteName = g.Key.Site.SiteName,
                TransactionCount = g.Count()
            })
            .ToList();

        return new AdminReportDto
        {
            Date = date,
            ActiveSitesCount = sites.Count,
            ActiveUsersCount = users.Count,
            ActiveSites = sites,
            ActiveUsers = users
        };
    }

    public async Task<List<StockSummaryDto>> GetStockSummaryAsync(int siteId)
    {
        return await _db.Items
            .Where(i => i.SiteId == siteId && i.IsActive == true)
            .OrderBy(i => i.ItemName)
            .Select(i => new StockSummaryDto
            {
                ItemId = i.ItemId,
                ItemName = i.ItemName,
                Unit = i.Unit,
                CurrentQuantity = i.CurrentQuantity,
                MinimumQuantity = i.MinimumQuantity,
                StockStatus = i.CurrentQuantity <= i.MinimumQuantity ? "LOW" : "OK"
            })
            .ToListAsync();
    }

    public byte[] GenerateDailyPdf(DailyReportDto report)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);

                page.Content().Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Text($"Daily Stock Report - {report.Date:yyyy-MM-dd}")
                        .FontSize(16)
                        .SemiBold();

                    column.Item().Text($"Total IN: {report.TotalInQuantity}");
                    column.Item().Text($"Total OUT: {report.TotalOutQuantity}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Time").SemiBold();
                            header.Cell().Text("Item").SemiBold();
                            header.Cell().Text("Type").SemiBold();
                            header.Cell().Text("Qty").SemiBold();
                            header.Cell().Text("Supplier").SemiBold();
                            header.Cell().Text("Recorded By").SemiBold();
                        });

                        foreach (var transaction in report.Transactions)
                        {
                            table.Cell().Text(transaction.TransactionDate.ToString("HH:mm"));
                            table.Cell().Text(transaction.ItemName);
                            table.Cell().Text(transaction.TransactionType);
                            table.Cell().Text(transaction.Quantity.ToString());
                            table.Cell().Text(transaction.SupplierName ?? "-");
                            table.Cell().Text(transaction.RecordedBy);
                        }
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
