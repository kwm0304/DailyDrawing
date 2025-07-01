using Server.Entities;

namespace Server.Services.Interfaces;

public interface IRefreshTokenService
{
  Task<RefreshToken?> GetByTokenAsync(string token);
  Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
  Task AddAsync(RefreshToken refreshToken);
  Task UpdateAsync(RefreshToken refreshToken);
  Task RemoveExpiredTokensAsync();
  Task RevokeAllUserTokensAsync(string userId, string revokedBy, string ipAddress);
}
