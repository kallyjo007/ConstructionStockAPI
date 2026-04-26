namespace ConstructionStockAPI.DTOs;

public class CreateSiteDto
{
    public string SiteName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int[]? AssignUserIds { get; set; }
}

public class SiteResponseDto
{
    public int SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

public class UpdateUserSiteDto
{
    public int SiteId { get; set; }
}
