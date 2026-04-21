namespace ConstructionStockAPI.DTOs;

public class RecordTransactionDto
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "IN" or "OUT"
    public int? SupplierId { get; set; } // only for IN
    public string? Remarks { get; set; }
}

public class TransactionResponseDto
{
    public int TransactionId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? SupplierName { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class StockStatusDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty; // "OK" or "LOW"
}
