using System.ComponentModel.DataAnnotations;

namespace Server.Entities;

public class Follow
{
  [Required]
  public required string FollowerId { get; set; }
  [Required]
  public required AppUser Follower { get; set; }
  [Required]
  public required string FollowingId { get; set; }
  [Required]
  public required AppUser Following { get; set; }
  public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
}
 