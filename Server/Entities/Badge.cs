using System.ComponentModel.DataAnnotations;

namespace Server.Entities;

public class Badge
{
  [Key]
  public int Id { get; set; }
  public required string BadgeName { get; set; }

  //nav
  public required string UserId { get; set; }
  public required AppUser User { get; set; }
  public int DrawingId { get; set; }
  public required Drawing Drawing { get; set; }
}
