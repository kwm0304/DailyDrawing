namespace Server.DTOs;

public class TokenResponse
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public DateTime RefreshExpiresAt { get; set; }
  public string TokenType { get; set; } = "Bearer";
}

public record RefreshTokenDto(string RefreshToken);
public record RevokeTokenDto(string RefreshToken);