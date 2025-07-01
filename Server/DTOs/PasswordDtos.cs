using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public record ChangePasswordDto
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  public string Email { get; init; } = string.Empty;

  [Required(ErrorMessage = "Token is required")]
  public string Token { get; init; } = string.Empty;

  [Required(ErrorMessage = "New password is required")]
  [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
  public string NewPassword { get; init; } = string.Empty;
}

public record ForgotPasswordDto
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  public string Email { get; init; } = string.Empty;
}

public record ResetPasswordLinkDto(string Email, string DisplayName, string ResetUrl);

public class ResetPasswordDto
{
  [Required(ErrorMessage = "User ID is required")]
  public string UserId { get; init; } = string.Empty;

  [Required(ErrorMessage = "Token is required")]
  public string Token { get; init; } = string.Empty;

  [Required(ErrorMessage = "New password is required")]
  [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
  public string NewPassword { get; init; } = string.Empty;
}