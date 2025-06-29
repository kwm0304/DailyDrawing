using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.DTOs;
using Server.Entities;
using Server.Mappers;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrawingController(Mapper mapper, IDrawingService drawingService, UserManager<AppUser> userManager, ILogger<DrawingController> logger) : ControllerBase
{
  private readonly ILogger<DrawingController> _logger = logger;
  private readonly IDrawingService _drawingService = drawingService;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly Mapper _mapper = mapper;
  
  [HttpGet("{displayName}/all")]
  public async Task<ActionResult<PagedResponse<DrawingDto>>> GetAllDrawingsForUser(
    [FromQuery] string displayName, [FromQuery] int page = 1, [FromQuery] int pageSize = 24)
  {
    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.DisplayName == displayName);
    if (user == null) return BadRequest("User not found");

    var drawings = await _drawingService.GetAllPaginatedDrawingsForUser(user.Id, page, pageSize);
    return Ok(drawings);
  }

  [HttpGet("{drawingId}")]
  public async Task<ActionResult<DrawingDto>> GetDrawingById([FromQuery] int drawingId)
  {
    var drawing = await _drawingService.GetDrawingDtoById(drawingId);
    if (drawing != null) return Ok(drawing);
    return StatusCode(500, "Unable to get drawing");
  }

  [HttpPost]
  public async Task<IActionResult> SubmitDrawing([FromBody] DrawingUploadDto dto)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized();
    try
    {
      var result = await _drawingService.SubmitDrawing(dto, user);
      return Ok(new { message = "Drawing submitted successfully", drawingId = result.Id });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to submit drawing for user {UserId}", user.Id);
      return StatusCode(500, "Failed to submit drawing");
    }
  }

  [HttpPut("{drawingId}")]
  public async Task<IActionResult> UpdateDrawingById([FromBody] UpdateDrawingDto dto, int drawingId)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Unauthorized();
    try
    {
      var updated = await _drawingService.UpdateDrawingById(userId, drawingId, dto);
      if (updated == null) return NotFound("Drawing not found");
      var response = _mapper.ConvertDrawingToDto(updated);
      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update drawing {DrawingId} for user {UserId}", drawingId, userId);
      return StatusCode(500, "Failed to update drawing");
    }
  }

  [HttpDelete("{drawingId}")]
  public async Task<IActionResult> DeleteDrawingById([FromQuery] int drawingId)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return BadRequest("No user found with this id");

    var result = await _drawingService.DeleteDrawingById(userId, drawingId);
    return Ok(result);
  }
}