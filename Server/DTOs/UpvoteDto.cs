using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public class UpvoteDto
{
  [Required]
  public int DrawingId { get; set; }
  [Required]
  public required string UserId { get; set; }
}
