using System.ComponentModel.DataAnnotations;

namespace ConstructionStockAPI.DTOs;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public int UserId { get; set; }
    public string SiteName { get; set; } = string.Empty;
}
