using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.DTOs.Auth;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService accountService, ILogger<AccountController> logger) : ControllerBase
{
  private readonly IAccountService _accountService = accountService;
  private readonly ILogger<AccountController> _logger = logger;
  #region register/login
  [HttpPost("login")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);
    var result = await _accountService.LoginAsync(dto);
    if (!result.IsSuccess)
      return Unauthorized(new { message = result.Errors.FirstOrDefault() });
    return Ok(result.Data);
  }

  [HttpPost("register")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var result = await _accountService.RegisterAsync(dto);
    if (!result.IsSuccess)
      return BadRequest(new { message = "Registration failed", errors = result.Errors });

    return CreatedAtAction(nameof(Register), result.Data);
  }
  #endregion
  [HttpPost("forgot-password")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var result = await _accountService.ForgotPasswordAsync(dto);
    if (!result.IsSuccess)
      return StatusCode(500, new { message = result.Errors.FirstOrDefault() });

    return Ok(new { message = result.Data });
  }

  [HttpPost("reset-password")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var result = await _accountService.ResetPasswordAsync(dto);
    if (!result.IsSuccess)
      return BadRequest(new { message = "Password reset failed", errors = result.Errors });

    return Ok(new { message = result.Data });
  }

  [HttpPost("change-password")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var result = await _accountService.ChangePasswordAsync(dto);
    if (!result.IsSuccess)
    {
      var firstError = result.Errors.FirstOrDefault();
      if (firstError?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
        return NotFound(new { message = firstError });

      return BadRequest(new { message = "Password change failed", errors = result.Errors });
    }

    return Ok(new { message = result.Data });
  }

  [HttpDelete("delete-account")]
  [Authorize]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
      return Unauthorized();

    var result = await _accountService.DeleteAccountAsync(userId, dto.Password);
    if (!result.IsSuccess)
    {
      var firstError = result.Errors.FirstOrDefault();
      if (firstError?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
        return NotFound(new { message = firstError });
      if (firstError?.Contains("Invalid password", StringComparison.OrdinalIgnoreCase) == true)
        return BadRequest(new { message = firstError });

      return StatusCode(500, new { message = "An error occurred during account deletion" });
    }

    return Ok(new { message = result.Data });
  }
}
