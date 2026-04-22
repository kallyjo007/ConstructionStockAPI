using System.Security.Claims;
using ConstructionStockAPI.Helpers;
using ConstructionStockAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "StockManager")]
public class AlertsController : ControllerBase
{
    private readonly AlertService _alertService;

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
    }

    private int GetSiteId() => int.Parse(User.FindFirstValue("SiteId")!);

    [HttpGet]
    public async Task<IActionResult> GetUnresolvedAlerts()
    {
        var alerts = await _alertService.GetUnresolvedAlertsAsync(GetSiteId());
        return Ok(ApiResponse<object>.Ok(alerts));
    }

    [HttpPut("{id:int}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id)
    {
        var resolved = await _alertService.ResolveAlertAsync(GetSiteId(), id);
        if (!resolved)
            return NotFound(ApiResponse<object>.Fail("Alert not found or already resolved."));

        return Ok(ApiResponse<object>.Ok(new { alertId = id }, "Alert resolved successfully."));
    }
}
