namespace Server.DTOs;

public class TagDto
{
  public string TagType { get; set; } = string.Empty;
  public string TagName { get; set; } = string.Empty;
  public string? CustomTagName { get; set; }
}
