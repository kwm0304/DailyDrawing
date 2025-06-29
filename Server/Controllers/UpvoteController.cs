using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Entities;
using Server.Services;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpvoteController(AppDbContext context, IDrawingService drawingService, UserManager<AppUser> userManager) : ControllerBase
{
  private readonly AppDbContext _context = context;
  private readonly IDrawingService _drawingService = drawingService;
  private readonly UserManager<AppUser> _userManager = userManager;

  [HttpPost]
  public async Task<IActionResult> CreateUpvote([FromBody] UpvoteDto dto)
  {
    var user = await _userManager.Users.FirstOrDefaultAsync(au => au.Id == dto.UserId);
    if (user == null) return Unauthorized();

    var drawing = await _drawingService.GetDrawingById(dto.DrawingId);
    if (drawing == null) return BadRequest("Drawing with this Id not found");
    var upvote = new Upvote
    {
      User = user,
      AppUserId = user.Id,
      Drawing = drawing,
      DrawingId = drawing.Id
    };
    _context.Upvotes.Add(upvote);
    await _context.SaveChangesAsync();
    return Ok();
  }
  [HttpDelete]
  public async Task<IActionResult> DeleteUpvote([FromBody] UpvoteDto dto)
  {
    await _context.Upvotes.Where(u => u.AppUserId == dto.UserId && u.DrawingId == dto.DrawingId).ExecuteDeleteAsync();
    return Ok();
  }
}