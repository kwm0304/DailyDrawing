using Server.DTOs;
using Server.DTOs.Auth;

namespace Server.Services.Interfaces;

public interface IAccountService
{
  Task<Result<AuthResponse>> LoginAsync(LoginDto dto);
  Task<Result<AuthResponse>> RegisterAsync(RegisterDto dto);
  Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
  Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto);
  Task<Result<string>> ChangePasswordAsync(ChangePasswordDto dto);
  Task<Result<string>> DeleteAccountAsync(string userId, string password);
}