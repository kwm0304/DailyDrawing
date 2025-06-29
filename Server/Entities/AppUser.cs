using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Server.Enums;

namespace Server.Entities;

public class AppUser : IdentityUser
{
  [Required]
  public required string DisplayName { get; set; }

  public ExperienceLevel ExperienceLevel { get; set; }
  public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
  public List<string> SocialMediaLinks { get; set; } = [];
  public ICollection<Drawing> Uploads { get; set; } = [];
  public ICollection<Upvote> Upvotes { get; set; } = [];
  public ICollection<Badge> Badges { get; set; } = [];
  public ICollection<Follow> Followers { get; set; } = [];
  public ICollection<Follow> Following { get; set; } = [];
}
