using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public class ChangePasswordDto
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  [Display(Name = "Email")]
  public required string Email { get; set; }
  [Required]
  public required string Token { get; set; }
  [Required]
  public required string OldPassword { get; set; }
  [Required]
  public required string NewPassword { get; set; }
  [Required]
  public required string ConfirmNewPassword { get; set; }
}

public class ForgotPasswordDto
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  [Display(Name = "Email")]
  public required string Email { get; set; }
}

public record ResetPasswordLinkDto(string Email, string DisplayName, string ResetUrl);

public class ResetPasswordDto
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string Token { get; set; } = string.Empty;

  [Required(ErrorMessage = "Password is required")]
  [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be 6-100 characters")]
  public string NewPassword { get; set; } = string.Empty;
}