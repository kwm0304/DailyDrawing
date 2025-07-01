using System.Security.Claims;
using Server.DTOs;
using Server.Entities;

namespace Server.Services.Interfaces;

public interface ITokenService
{
  TokenResponse GenerateToken(AppUser user, string? ipAddress = null);
  Task<TokenResponse?> RefreshTokenAsync(string token, string ipAddress);
  Task<bool> RevokeTokenAsync(string token, string ipAddress);
  Task RevokeAllUserTokensAsync(string userId, string ipAddress);
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
  Task CleanupExpiredTokensAsync();
}
