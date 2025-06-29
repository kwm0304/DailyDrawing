using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.DTOs;
using Server.DTOs.Auth;
using Server.Entities;
using Server.Services;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(
  UserManager<AppUser> userManager,
  SignInManager<AppUser> signInManager,
  ILogger<AccountController> logger,
  ITokenService tokenService,
  IConfiguration config,
  IEmailService emailService,
  IUserService userService,
  IDrawingService drawingService,
  IS3Service s3Service) : ControllerBase
{
  private readonly IConfiguration _config = config;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly SignInManager<AppUser> _signInManager = signInManager;
  private readonly ILogger<AccountController> _logger = logger;
  private readonly ITokenService _tokenService = tokenService;
  private readonly IEmailService _emailService = emailService;
  private readonly IUserService _userService = userService;
  private readonly IDrawingService _drawingService = drawingService;
  private readonly IS3Service _s3Service = s3Service;
  #region register/login
  [HttpPost("login")]
  public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto dto)
  {
    if (dto == null) return BadRequest("Invalid login request");
    if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is a required field");
    if (string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("Password is a required field");

    var user = await _userManager.FindByNameAsync(dto.Email);
    if (user == null || user.UserName == null) return Unauthorized("Invalid username");
    var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
    if (!result.Succeeded) return Unauthorized("Invalid email or password");
    _logger.LogInformation("User logged in successfully: {Email}", user.Email);
    return new AuthResponse
    {
      Token = _tokenService.GenerateToken(user),
      Email = user.Email!,
      DisplayName = user.DisplayName
    };
  }

  [HttpPost("register")]
  public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto dto)
  {
    if (dto == null) return BadRequest("Invalid registration request");
    var userExists = await UserExists(dto.Email);
    if (userExists) return BadRequest("User with this email already exists");
    var user = new AppUser
    {
      DisplayName = dto.DisplayName,
      Email = dto.Email
    };
    var result = await _userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded) return BadRequest(result.Errors);
    _logger.LogInformation("User registered successfully: {Email}", user.Email);
    return new AuthResponse
    {
      Token = _tokenService.GenerateToken(user),
      Email = dto.Email,
      DisplayName = dto.DisplayName
    };
  }
  #endregion

  private async Task<bool> UserExists(string username)
  {
    return await _userManager.Users.AnyAsync(u => u.UserName == username);
  }
  #region password
  [HttpPost("change-password")]
  public async Task<IActionResult> ChagePassword([FromBody] ChangePasswordDto dto)
  {
    if (dto == null || !ModelState.IsValid) return BadRequest("Invalid change password request");
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user == null) return NotFound("No user found with this email");
    //generate reset token
    var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
    if (!result.Succeeded)
    {
      return BadRequest(result.Errors);
    }
    _logger.LogInformation("Password reset requested for user: {Email}", user.Email);
    return Ok("Password reset link has been sent to your email");
  }

  [HttpPost("forgot-password")] //failed num of attempts
  public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
  {
    if (!ModelState.IsValid) return StatusCode(500, "Something went wrong processing your request");
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user != null)
    {
      var confirmed = await _userManager.IsEmailConfirmedAsync(user);
      if (confirmed)
      {
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
      }
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      var resetUrl = $"{_config["Client:BaseUrl"]}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";
      await _emailService.SendPasswordResetEmailAsync(new ResetPasswordLinkDto(user.Email!, user.DisplayName, resetUrl));

      return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }
    else
    {
      return StatusCode(500, "Something went wrong processing your request");
    }
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var user = await _userManager.FindByIdAsync(dto.UserId);
    if (user == null) return BadRequest(new { message = "Invalid password request sent" });

    var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
    if (result.Succeeded) return Ok(new { message = "Password has been reset successfully" });

    var errors = result.Errors.Select(e => e.Description).ToList();
    return BadRequest(new { message = "Password reset failed", errors });
  }
  #endregion
  [HttpDelete("delete-account")]
  [Authorize]
  public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId != null)
    {
      var user = await _userManager.FindByIdAsync(userId);

      if (user == null)
        return NotFound(new { message = "User not found" });

      var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
      if (!passwordValid)
        return BadRequest(new { message = "Invalid password" });
      var drawings = await _drawingService.GetAllUserDrawings(user.Id);
      await _s3Service.DeleteUserDrawingFilesAsync(drawings);
      await _userService.CleanupUserData(user.Id);
      return Ok("Account deleted successfully");
    }
    else
    {
      return BadRequest("No user found with this Id");
    }
  }

}
