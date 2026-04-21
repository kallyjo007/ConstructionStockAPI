using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Models;

[Index("SiteId", "IsResolved", Name = "IX_Alerts_SiteId_Resolved")]
public partial class LowStockAlert
{
    [Key]
    public int AlertId { get; set; }

    public int ItemId { get; set; }

    public int SiteId { get; set; }

    public int QuantityAtAlert { get; set; }

    public bool IsResolved { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime AlertDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ResolvedAt { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("LowStockAlerts")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("SiteId")]
    [InverseProperty("LowStockAlerts")]
    public virtual Site Site { get; set; } = null!;
}
