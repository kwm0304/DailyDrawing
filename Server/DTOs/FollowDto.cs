using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public class FollowDto
{
  [Required]
  public string FollowerId { get; set; } = string.Empty;
  [Required]
  public string FollowingId { get; set; } = string.Empty;
}
