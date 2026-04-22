using System.Security.Claims;
using ConstructionStockAPI.Helpers;
using ConstructionStockAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "StockManager")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    private int GetSiteId() => int.Parse(User.FindFirstValue("SiteId")!);

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateOnly? date)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var report = await _reportService.GetDailyReportAsync(GetSiteId(), reportDate);
        return Ok(ApiResponse<object>.Ok(report));
    }

    [HttpGet("daily/export")]
    public async Task<IActionResult> ExportDailyReport([FromQuery] DateOnly? date)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var report = await _reportService.GetDailyReportAsync(GetSiteId(), reportDate);
        var pdfBytes = _reportService.GenerateDailyPdf(report);
        var fileName = $"daily-report-{reportDate:yyyy-MM-dd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("stock-summary")]
    public async Task<IActionResult> GetStockSummary()
    {
        var summary = await _reportService.GetStockSummaryAsync(GetSiteId());
        return Ok(ApiResponse<object>.Ok(summary));
    }
}
