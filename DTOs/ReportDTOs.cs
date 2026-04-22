namespace ConstructionStockAPI.DTOs;

public class DailyReportTransactionDto
{
    public int TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? SupplierName { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}

public class DailyReportDto
{
    public DateOnly Date { get; set; }
    public int TotalInQuantity { get; set; }
    public int TotalOutQuantity { get; set; }
    public List<DailyReportTransactionDto> Transactions { get; set; } = [];
}

public class StockSummaryDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty;
}
