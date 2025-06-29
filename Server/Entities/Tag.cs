using Server.Enums;

namespace Server.Entities;

public class Tag
{
  public TagTypes TagType { get; set; }
  public string TagName { get; set; } = string.Empty;
  public Tag(){}
  public Tag(TagTypes tagType, string? customName = null)
  {
    TagType = tagType;
    TagName = (tagType == TagTypes.Custom && !string.IsNullOrWhiteSpace(customName))
      ? customName
      : tagType.ToString();
  }
}