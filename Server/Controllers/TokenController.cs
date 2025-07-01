using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController(ITokenService tokenService, ILogger<TokenController> logger) : ControllerBase
{
  private readonly ITokenService _tokenService = tokenService;
  private readonly ILogger<TokenController> _logger = logger;
  [HttpPost("refresh")]
  public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenDto dto)
  {
    var ipAddress = GetIpAddress();
    var tokenResponse = await _tokenService.RefreshTokenAsync(dto.RefreshToken, ipAddress);
    if (tokenResponse == null)
      return Unauthorized(new { message = "Invalid refresh token" });
    return Ok(tokenResponse);
  }

  private string GetIpAddress()
  {
    return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
  }

  [HttpPost("revoke")]
  [Authorize]
  public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto dto)
  {
    var ipAddress = GetIpAddress();
    var success = await _tokenService.RevokeTokenAsync(dto.RefreshToken, ipAddress);
    if (!success)
      return BadRequest(new { message = "Token revocation failed" });
    return Ok(new { message = "Token revoked successfully" });
  }

  [HttpPost("revoke-all")]
  [Authorize]
  public async Task<IActionResult> RevokeAllTokens()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
      return Unauthorized();
    var ipAddress = GetIpAddress();
    await _tokenService.RevokeAllUserTokensAsync(userId, ipAddress);
    return Ok(new { message = "All tokens revoked successfully" });
  }

  
}
