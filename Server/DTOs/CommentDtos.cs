using System.ComponentModel.DataAnnotations;
using Server.Entities;

namespace Server.DTOs;

public class CreateCommentDto
{
  public string Content { get; set; } = string.Empty;
  public int? ParentCommentId { get; set; }
  public Comment? ParentComment { get; set; }
  public int DrawingId { get; set; }
  [Required]
  public required string UserId { get; set; }
}

public class UpdateCommentDto
{
  [Required]
  public int CommentId { get; set; }
  [StringLength(280, ErrorMessage = "Comments must be 280 characters or less")]
  public string NewContent { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  [Required]
  public required string DrawingId { get; set; }
}