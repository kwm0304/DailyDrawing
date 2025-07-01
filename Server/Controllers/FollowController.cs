using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowController(IFollowService followService, UserManager<AppUser> userManager, IConfiguration config, ILogger<FollowController> logger) : ControllerBase
{
  private readonly IFollowService _followService = followService;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly IConfiguration _config = config;
  private readonly ILogger<FollowController> _logger = logger;


  [HttpPost]
  public async Task<IActionResult> CreateFollow([FromBody] FollowDto dto)
  {
    var follower = await _userManager.GetUserAsync(User);
    if (follower == null) return NotFound("User not found");

    var following = await _userManager.FindByIdAsync(dto.FollowingId);
    if (following == null) return NotFound("User not found");

    var follow = await _followService.CreateFollow(follower, following);
    return Ok();
  }

  [HttpDelete]
  public async Task<IActionResult> DeleteFollow([FromBody] FollowDto dto)
  {
    var user = _userManager.GetUserAsync(User);
    return Ok();
  }

  [HttpGet]
  public async Task<IActionResult> GetAllFollowersForUser()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return NotFound("No user found with this Id");

    return Ok();
  }
}
