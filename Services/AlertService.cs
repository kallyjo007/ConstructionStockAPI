using ConstructionStockAPI.Data;
using ConstructionStockAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Services;

public class AlertService
{
    private readonly ConstructionStockDbContext _db;

    public AlertService(ConstructionStockDbContext db)
    {
        _db = db;
    }

    public async Task<List<AlertResponseDto>> GetUnresolvedAlertsAsync(int siteId)
    {
        return await _db.LowStockAlerts
            .Where(a => a.SiteId == siteId && a.IsResolved == false)
            .OrderByDescending(a => a.AlertDate)
            .Select(a => new AlertResponseDto
            {
                AlertId = a.AlertId,
                ItemId = a.ItemId,
                ItemName = a.Item.ItemName,
                QuantityAtAlert = a.QuantityAtAlert,
                MinimumQuantity = a.Item.MinimumQuantity,
                AlertDate = a.AlertDate
            })
            .ToListAsync();
    }

    public async Task<bool> ResolveAlertAsync(int siteId, int alertId)
    {
        var alert = await _db.LowStockAlerts
            .FirstOrDefaultAsync(a => a.AlertId == alertId && a.SiteId == siteId);

        if (alert == null || alert.IsResolved)
            return false;

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return true;
    }
}
