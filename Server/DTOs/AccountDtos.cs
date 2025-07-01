using System.ComponentModel.DataAnnotations;

namespace Server.DTOs.Auth;

public record RegisterDto
{
  [Required(ErrorMessage = "Display name is required")]
  [StringLength(50, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 50 characters")]
  public string DisplayName { get; set; } = string.Empty;
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
  public string Email { get; init; } = string.Empty;
  [Required(ErrorMessage = "Password is required")]
  [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
  [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
  public string Password { get; init; } = string.Empty;

}
public record LoginDto
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  public string Email { get; init; } = string.Empty;

  [Required(ErrorMessage = "Password is required")]
  public string Password { get; init; } = string.Empty;
}
public record AuthResponse
{
  [Required]
  public required string DisplayName { get; set; }

  [Required]
  public required string Token { get; set; }

  [Required]
  [EmailAddress]
  public required string Email { get; set; }

  public DateTime ExpiresAt { get; set; }

  public string RefreshToken { get; set; } = string.Empty;
};
public record DeleteAccountDto
{
  [Required(ErrorMessage = "Password is required")]
  public string Password { get; init; } = string.Empty;
}