using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Entities;
using Server.Mappers;
using Server.Services;
using Server.Services.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController(DrawingService drawingService, ICommentService commentService, UserManager<AppUser> userManager, ILogger<CommentController> logger) : ControllerBase
{
  private readonly ICommentService _commentService = commentService;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly ILogger<CommentController> _logger = logger;
  private readonly DrawingService _drawingService = drawingService;

  //getall
  //getbyid
  //create
  //update
  //delete
  [HttpGet("drawing/{drawingId}")]
  public async Task<ActionResult<PagedResponse<Comment>>> GetCommentsForDrawing(int drawingId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
  {
    var comments = await _commentService.GetCommentsForDrawing(drawingId, page, pageSize);
    return Ok(comments);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Comment>> GetCommentById(int id)
  {
    var comment = await _commentService.GetCommentById(id);
    if (comment == null) return NotFound();
    return Ok(comment);
  }
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteComment(int id)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var success = await _commentService.DeleteCommentAsync(id, userId);
    if (!success) return NotFound();
    return NoContent();
  }
  [HttpPost]
  public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized("You must be logged in to comment");

    var drawing = await _drawingService.GetDrawingById(dto.DrawingId);
    if (drawing == null) return NotFound("No drawing found with this id");

    var created = await _commentService.CreateComment(user, dto, drawing);
    return CreatedAtAction(nameof(GetCommentById), new { Id = created.Id });
  }

  [HttpPut]
  public async Task<IActionResult> UpdateCommentById([FromBody] UpdateCommentDto dto)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId != dto.UserId) return Unauthorized();

    var result = await _commentService.UpdateCommentById(dto);
    return Ok(result);
  }
}
