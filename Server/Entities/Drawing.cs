using System.ComponentModel.DataAnnotations;

namespace Server.Entities;

public class Drawing
{
  [Key]
  public int Id { get; set; }
  [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
  public required string Title { get; set; }
  [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
  public string Description { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; }
  public ICollection<Comment> Comments { get; set; } = [];
  public ICollection<Upvote> Upvotes { get; set; } = [];
  public ICollection<Tag> Tags { get; set; } = [];
  //s3 info
  public string FileName { get; set; } = string.Empty;
  public string S3Key { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long FileSize { get; set; }
  //nav
  public required string UserId { get; set; }
  public required AppUser User { get; set; }
  public ICollection<Badge> Badges { get; set; } = [];
}
