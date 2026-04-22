namespace ConstructionStockAPI.DTOs;

public class AlertResponseDto
{
    public int AlertId { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int QuantityAtAlert { get; set; }
    public int MinimumQuantity { get; set; }
    public DateTime AlertDate { get; set; }
}
