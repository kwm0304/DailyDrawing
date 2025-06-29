using System.ComponentModel.DataAnnotations;

namespace Server.Entities;

public class Comment
{
  [Key]
  public int Id { get; set; }
  [Required]
  public required string Content { get; set; }
  public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; }
  public int? ParentCommentId { get; set; }
  public Comment? ParentComment { get; set; }
  public ICollection<Comment> Replies { get; set; } = [];

  //nav
  public int DrawingId { get; set; }
  [Required]
  public required Drawing Drawing { get; set; }

  public required string UserId { get; set; }
  [Required]
  public required AppUser User { get; set; }
}
