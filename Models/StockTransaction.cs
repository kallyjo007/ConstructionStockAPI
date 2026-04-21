using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Models;

[Index("ItemId", Name = "IX_Stock_ItemId")]
[Index("RecordedByUserId", Name = "IX_Stock_RecordedByUserId")]
[Index("SiteId", Name = "IX_Stock_SiteId")]
[Index("SupplierId", Name = "IX_Stock_SupplierId")]
public partial class StockTransaction
{
    [Key]
    public int TransactionId { get; set; }

    public int ItemId { get; set; }

    public int SiteId { get; set; }

    public int RecordedByUserId { get; set; }

    public int? SupplierId { get; set; }

    [StringLength(3)]
    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TransactionDate { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("StockTransactions")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("RecordedByUserId")]
    [InverseProperty("StockTransactions")]
    public virtual User RecordedByUser { get; set; } = null!;

    [ForeignKey("SiteId")]
    [InverseProperty("StockTransactions")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("SupplierId")]
    [InverseProperty("StockTransactions")]
    public virtual Supplier? Supplier { get; set; }
}
