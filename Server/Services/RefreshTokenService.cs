using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class RefreshTokenService(AppDbContext context, ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
  private readonly AppDbContext _context = context;
  private readonly ILogger<RefreshTokenService> _logger = logger;

  public async Task AddAsync(RefreshToken refreshToken)
  {
    await _context.RefreshTokens.AddAsync(refreshToken);
    await _context.SaveChangesAsync();
  }

  public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
  {
    return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
  }

  public async Task<RefreshToken?> GetByTokenAsync(string token)
  {
    return await _context.RefreshTokens
      .Include(r => r.User)
      .FirstOrDefaultAsync(r => r.Token == token);
  }

  public async Task RemoveExpiredTokensAsync()
  {
    var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

    if (expiredTokens.Any())
    {
      _context.RefreshTokens.RemoveRange(expiredTokens);
      await _context.SaveChangesAsync();
      _logger.LogInformation("Removed {Count} expired refresh tokens", expiredTokens.Count);
    }
  }

  public async Task RevokeAllUserTokensAsync(string userId, string revokedBy, string ipAddress)
  {
    var activeTokens = await GetActiveTokensByUserIdAsync(userId);

    foreach (var token in activeTokens)
    {
      token.IsRevoked = true;
      token.RevokedAt = DateTime.UtcNow;
      token.RevokedBy = revokedBy;
      token.RevokedByIp = ipAddress;
    }

    if (activeTokens.Any())
    {
      await _context.SaveChangesAsync();
      _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", activeTokens.Count, userId);
    }
  }

  public async Task UpdateAsync(RefreshToken refreshToken)
  {
    _context.RefreshTokens.Update(refreshToken);
    await _context.SaveChangesAsync();
  }

}
