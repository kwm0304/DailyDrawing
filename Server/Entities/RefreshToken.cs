namespace Server.Entities;

public class RefreshToken
{
  public int Id { get; set; }
  public string Token { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public bool IsRevoked { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public string? RevokedBy { get; set; }
  public DateTime? RevokedAt { get; set; }
  public string? ReplacedByToken { get; set; }
  public string CreatedByIp { get; set; } = string.Empty;
  public string? RevokedByIp { get; set; }
  public AppUser User { get; set; } = null!;
  public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
  public bool IsActive => !IsRevoked && !IsExpired;
}
