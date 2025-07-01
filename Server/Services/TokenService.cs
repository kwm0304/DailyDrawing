using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.DTOs;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class TokenService(
    IConfiguration config,
    IRefreshTokenService refreshService,
    ILogger<TokenService> logger) : ITokenService
{
  private readonly IConfiguration _config = config;
  private readonly IRefreshTokenService _refreshService = refreshService;
  private readonly ILogger<TokenService> _logger = logger;
  private readonly TokenValidationParameters _tokenValidationParameters = CreateTokenValidationParameters(config);

  public TokenResponse GenerateToken(AppUser user, string? ipAddress = null)
  {
    if (string.IsNullOrEmpty(user.UserName))
      throw new ArgumentException("Username cannot be null or empty", nameof(user));

    var tokenKey = GetTokenKey();
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("display_name", user.DisplayName),
            new("experience_level", user.ExperienceLevel.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    var accessTokenExpiry = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes());

    var descriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = accessTokenExpiry,
      SigningCredentials = creds,
      Issuer = _config["JWT:Issuer"],
      Audience = _config["JWT:Audience"]
    };
    var handler = new JwtSecurityTokenHandler();
    var accessToken = handler.CreateToken(descriptor);
    var accessTokenString = handler.WriteToken(accessToken);
    var refreshToken = GenerateRefreshToken();
    var refreshTokenExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

    var refreshTokenEntity = new RefreshToken
    {
      Token = refreshToken,
      UserId = user.Id,
      ExpiresAt = refreshTokenExpiry,
      CreatedByIp = ipAddress ?? "unknown"
    };

    Task.Run(async () => await _refreshService.AddAsync(refreshTokenEntity));

    _logger.LogInformation("Generated new token for user {UserId}", user.Id);

    return new TokenResponse
    {
      AccessToken = accessTokenString,
      RefreshToken = refreshToken,
      ExpiresAt = accessTokenExpiry,
      RefreshExpiresAt = refreshTokenExpiry
    };
  }

  public async Task<TokenResponse?> RefreshTokenAsync(string token, string ipAddress)
  {
    try
    {
      var refreshToken = await _refreshService.GetByTokenAsync(token);

      if (refreshToken?.User == null)
      {
        _logger.LogWarning("Attempted to refresh invalid token from IP {IpAddress}", ipAddress);
        return null;
      }

      if (!refreshToken.IsActive)
      {
        _logger.LogWarning("Attempted to use inactive refresh token for user {UserId} from IP {IpAddress}",
            refreshToken.UserId, ipAddress);
        return null;
      }

      var newRefreshToken = GenerateRefreshToken();
      var refreshTokenExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

      refreshToken.IsRevoked = true;
      refreshToken.RevokedAt = DateTime.UtcNow;
      refreshToken.RevokedByIp = ipAddress;
      refreshToken.ReplacedByToken = newRefreshToken;

      var newRefreshTokenEntity = new RefreshToken
      {
        Token = newRefreshToken,
        UserId = refreshToken.UserId,
        ExpiresAt = refreshTokenExpiry,
        CreatedByIp = ipAddress
      };

      await _refreshService.UpdateAsync(refreshToken);
      await _refreshService.AddAsync(newRefreshTokenEntity);

      var tokenResponse = GenerateToken(refreshToken.User, ipAddress);
      tokenResponse.RefreshToken = newRefreshToken;
      tokenResponse.RefreshExpiresAt = refreshTokenExpiry;

      _logger.LogInformation("Refreshed token for user {UserId}", refreshToken.UserId);
      return tokenResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error refreshing token from IP {IpAddress}", ipAddress);
      return null;
    }
  }

  public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
  {
    try
    {
      var refreshToken = await _refreshService.GetByTokenAsync(token);

      if (refreshToken == null || !refreshToken.IsActive)
        return false;

      refreshToken.IsRevoked = true;
      refreshToken.RevokedAt = DateTime.UtcNow;
      refreshToken.RevokedByIp = ipAddress;

      await _refreshService.UpdateAsync(refreshToken);

      _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error revoking token from IP {IpAddress}", ipAddress);
      return false;
    }
  }

  public async Task RevokeAllUserTokensAsync(string userId, string ipAddress)
  {
    await _refreshService.RevokeAllUserTokensAsync(userId, "system", ipAddress);
  }

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();

      var validationParameters = _tokenValidationParameters.Clone();
      validationParameters.ValidateLifetime = false;

      var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

      if (validatedToken is not JwtSecurityToken jwtToken ||
          !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
      {
        return null;
      }

      return principal;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting principal from expired token");
      return null;
    }
  }

  public async Task CleanupExpiredTokensAsync()
  {
    try
    {
      await _refreshService.RemoveExpiredTokensAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error cleaning up expired tokens");
    }
  }

  private static TokenValidationParameters CreateTokenValidationParameters(IConfiguration config)
  {
    var tokenKey = GetTokenKeyStatic(config);
    return new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
      ValidateIssuer = true,
      ValidIssuer = config["JWT:Issuer"],
      ValidateAudience = true,
      ValidAudience = config["JWT:Audience"],
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero,
      RequireExpirationTime = true
    };
  }

  private string GetTokenKey() => GetTokenKeyStatic(_config);

  private static string GetTokenKeyStatic(IConfiguration config)
  {
    var tokenKey = config["JWT:TokenKey"] ?? throw new InvalidOperationException("JWT TokenKey not found in configuration");

    if (tokenKey.Length < 64)
      throw new InvalidOperationException("JWT TokenKey must be at least 64 characters long for security");

    return tokenKey;
  }

  private int GetAccessTokenExpiryMinutes()
  {
    return _config.GetValue<int>("JWT:AccessTokenExpiryMinutes", 15);
  }

  private int GetRefreshTokenExpiryDays()
  {
    return _config.GetValue<int>("JWT:RefreshTokenExpiryDays", 7);
  }

  private static string GenerateRefreshToken()
  {
    var randomBytes = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
  }
}