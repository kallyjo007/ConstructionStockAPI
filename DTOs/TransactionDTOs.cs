using System.ComponentModel.DataAnnotations;

namespace ConstructionStockAPI.DTOs;

public class RecordTransactionDto
{
    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public string TransactionType { get; set; } = string.Empty; // "IN" or "OUT"

    public int? SupplierId { get; set; } // only for IN

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // When Storekeeper records IN, it will be unapproved until StockManager approves
    public bool? ApproveImmediately { get; set; } = null;
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
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
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
