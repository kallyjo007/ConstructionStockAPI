using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Models;

[Index("SiteId", Name = "IX_Users_SiteId")]
[Index("Username", Name = "UQ__Users__536C85E4DC9C5B8D", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    public int SiteId { get; set; }

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("SiteId")]
    [InverseProperty("Users")]
    public virtual Site Site { get; set; } = null!;

    [InverseProperty("RecordedByUser")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    [InverseProperty("ApprovedByUser")]
    public virtual ICollection<StockTransaction> ApprovedTransactions { get; set; } = new List<StockTransaction>();
}
