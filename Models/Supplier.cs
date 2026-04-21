using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Models;

[Index("IsActive", Name = "IX_Suppliers_Active")]
public partial class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [StringLength(200)]
    public string SupplierName { get; set; } = null!;

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Supplier")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
