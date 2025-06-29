using System.ComponentModel.DataAnnotations;

namespace Server.DTOs.Auth;

public record RegisterDto(string DisplayName, string Email, string Password);
public record LoginDto(string Email, string Password);
public class AuthResponse()
{
  [Required]
  public required string DisplayName { get; set; }
  public required string Token { get; set; }
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  public required string Email { get; set; }
};