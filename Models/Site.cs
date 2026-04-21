using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Models;

public partial class Site
{
    [Key]
    public int SiteId { get; set; }

    [StringLength(150)]
    public string SiteName { get; set; } = null!;

    [StringLength(255)]
    public string Location { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Site")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [InverseProperty("Site")]
    public virtual ICollection<LowStockAlert> LowStockAlerts { get; set; } = new List<LowStockAlert>();

    [InverseProperty("Site")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    [InverseProperty("Site")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
