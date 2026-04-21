using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Models;

[Index("SiteId", Name = "IX_Items_SiteId")]
public partial class Item
{
    [Key]
    public int ItemId { get; set; }

    public int SiteId { get; set; }

    [StringLength(200)]
    public string ItemName { get; set; } = null!;

    [StringLength(50)]
    public string Unit { get; set; } = null!;

    public int MinimumQuantity { get; set; }

    public int CurrentQuantity { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<LowStockAlert> LowStockAlerts { get; set; } = new List<LowStockAlert>();

    [ForeignKey("SiteId")]
    [InverseProperty("Items")]
    public virtual Site Site { get; set; } = null!;

    [InverseProperty("Item")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
