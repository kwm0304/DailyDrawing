using Server.Entities;

namespace Server.Services.Interfaces;

public interface ITokenService
{
  string GenerateToken(AppUser user);
}
