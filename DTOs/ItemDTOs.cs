using System.ComponentModel.DataAnnotations;

namespace ConstructionStockAPI.DTOs;

public class ItemResponseDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int MinimumQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty;
}

public class CreateItemDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Unit { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentQuantity { get; set; }
}

public class UpdateItemDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Unit { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }
}
