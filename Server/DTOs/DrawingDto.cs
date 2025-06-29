using System.ComponentModel.DataAnnotations;
using Server.Entities;

namespace Server.DTOs;

public class DrawingDto
{
  public int Id { get; set; }
  [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
  public required string Title { get; set; }
  [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
  public string Description { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public int UpvoteCount { get; set; }
  public int CommentCount { get; set; }
  public string FullImageUrl { get; set; } = string.Empty;
  public List<string> Tags { get; set; } = [];
  public List<Badge> Badges { get; set; } = [];
}

public class DrawingUploadDto
{
  [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
  public required string Title { get; set; }
  [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
  public string Description { get; set; } = string.Empty;
  public required IFormFile File { get; set; }
  public List<Tag> Tags { get; set; } = [];
}

public class UpdateDrawingDto
{
  [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
  public string NewTitle { get; set; } = string.Empty;
  [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
  public string NewDescription { get; set; } = string.Empty;
  public List<Tag> NewTags { get; set; } = [];
}