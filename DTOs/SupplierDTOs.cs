using System.ComponentModel.DataAnnotations;

namespace ConstructionStockAPI.DTOs;

public class SupplierResponseDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class SupplierDeliveryHistoryDto
{
    public int TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}

public class SupplierDetailDto : SupplierResponseDto
{
    public List<SupplierDeliveryHistoryDto> RecentDeliveries { get; set; } = [];
}

public class CreateSupplierDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string SupplierName { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class UpdateSupplierDto : CreateSupplierDto
{
}
