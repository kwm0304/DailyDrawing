namespace Server.Entities;

public class Upvote
{
  public required string AppUserId { get; set; }
  public required AppUser User { get; set; }

  public int DrawingId { get; set; }
  public required Drawing Drawing { get; set; }
}
