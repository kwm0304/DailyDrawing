using Microsoft.AspNetCore.Identity;
using Server.Data;
using Server.DTOs;
using Server.DTOs.Auth;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class AccountService(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IEmailService emailService, IUserService userService, IDrawingService drawingService, IS3Service s3Service, IConfiguration config, ILogger<AccountService> logger) : IAccountService
{
  private readonly AppDbContext _context = context;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly SignInManager<AppUser> _signInManager = signInManager;
  private readonly ITokenService _tokenService = tokenService;
  private readonly IEmailService _emailService = emailService;
  private readonly IUserService _userService = userService;
  private readonly IDrawingService _drawingService = drawingService;
  private readonly IS3Service _s3Service = s3Service;
  private readonly IConfiguration _config = config;
  private readonly ILogger<AccountService> _logger = logger;

  public async Task<Result<AuthResponse>> LoginAsync(LoginDto dto)
  {
    try
    {
      var user = await _userManager.FindByEmailAsync(dto.Email);
      if (user?.Email == null)
      {
        await Task.Delay(100); 
        return Result<AuthResponse>.Failure("Invalid email or password");
      }

      var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
      if (result.IsLockedOut)
        return Result<AuthResponse>.Failure("Account is locked due to multiple failed login attempts");

      if (!result.Succeeded)
        return Result<AuthResponse>.Failure("Invalid email or password");

      var token = _tokenService.GenerateToken(user);
      var authResponse = new AuthResponse
      {
        Token = token.AccessToken,
        Email = user.Email,
        DisplayName = user.DisplayName,
        ExpiresAt = token.ExpiresAt,
        RefreshToken = token.RefreshToken
      };

      _logger.LogInformation("User {UserId} logged in successfully", user.Id);
      return Result<AuthResponse>.Success(authResponse);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during login for email {Email}", dto.Email);
      return Result<AuthResponse>.Failure("An error occurred during login");
    }
  }

  public async Task<Result<AuthResponse>> RegisterAsync(RegisterDto dto)
  {
    try
    {
      var existingUser = await _userManager.FindByEmailAsync(dto.Email);
      if (existingUser != null)
        return Result<AuthResponse>.Failure("User with this email already exists");

      var user = new AppUser
      {
        DisplayName = dto.DisplayName,
        Email = dto.Email,
        UserName = dto.Email, 
        EmailConfirmed = false 
      };

      var result = await _userManager.CreateAsync(user, dto.Password);
      if (!result.Succeeded)
      {
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return Result<AuthResponse>.Failure(errors);
      }

      var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      var confirmationUrl = $"{_config["Client:BaseUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";

      // TODO: Send confirmation email

      var token = _tokenService.GenerateToken(user);
      var authResponse = new AuthResponse
      {
        Token = token.AccessToken,
        Email = user.Email,
        DisplayName = user.DisplayName,
        ExpiresAt = token.ExpiresAt,
        RefreshToken = token.RefreshToken
      };

      _logger.LogInformation("User {UserId} registered successfully", user.Id);
      return Result<AuthResponse>.Success(authResponse);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during registration for email {Email}", dto.Email);
      return Result<AuthResponse>.Failure("An error occurred during registration");
    }
  }

  public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
  {
    try
    {
      var user = await _userManager.FindByEmailAsync(dto.Email);
      const string successMessage = "If an account with that email exists, a password reset link has been sent.";
      if (user?.EmailConfirmed == true)
      {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = $"{_config["Client:BaseUrl"]}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendPasswordResetEmailAsync(new ResetPasswordLinkDto(user.Email!, user.DisplayName, resetUrl));
        _logger.LogInformation("Password reset email sent to user {UserId}", user.Id);
      }
      return Result<string>.Success(successMessage);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during forgot password for email {Email}", dto.Email);
      return Result<string>.Failure("An error occurred processing your request");
    }
  }

  public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto)
  {
    try
    {
      var user = await _userManager.FindByIdAsync(dto.UserId);
      if (user == null)
        return Result<string>.Failure("Invalid password reset request");

      var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
      if (!result.Succeeded)
      {
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return Result<string>.Failure(errors);
      }

      _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
      return Result<string>.Success("Password has been reset successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during password reset for user {UserId}", dto.UserId);
      return Result<string>.Failure("An error occurred during password reset");
    }
  }

  public async Task<Result<string>> ChangePasswordAsync(ChangePasswordDto dto)
  {
    try
    {
      var user = await _userManager.FindByEmailAsync(dto.Email);
      if (user == null)
        return Result<string>.Failure("User not found");

      var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
      if (!result.Succeeded)
      {
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return Result<string>.Failure(errors);
      }

      _logger.LogInformation("Password changed successfully for user {UserId}", user.Id);
      return Result<string>.Success("Password changed successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during password change for email {Email}", dto.Email);
      return Result<string>.Failure("An error occurred during password change");
    }
  }

  public async Task<Result<string>> DeleteAccountAsync(string userId, string password)
  {
    try
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
        return Result<string>.Failure("User not found");

      var passwordValid = await _userManager.CheckPasswordAsync(user, password);
      if (!passwordValid)
        return Result<string>.Failure("Invalid password");

      var drawings = await _drawingService.GetAllUserDrawings(user.Id);
      await _s3Service.DeleteUserDrawingFilesAsync(drawings);
      await _userService.CleanupUserData(user.Id);

      var result = await _userManager.DeleteAsync(user);
      if (!result.Succeeded)
      {
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return Result<string>.Failure(errors);
      }

      _logger.LogInformation("Account deleted successfully for user {UserId}", userId);
      return Result<string>.Success("Account deleted successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during account deletion for user {UserId}", userId);
      return Result<string>.Failure("An error occurred during account deletion");
    }
  }
}