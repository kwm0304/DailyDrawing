namespace Server.DTOs;

public class EmailDto
{
  public string RecipientEmail { get; set; } = string.Empty;
  public string RecipientDisplayName { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string MessageBody { get; set; } = string.Empty;
}
