using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class TokenService(IConfiguration config) : ITokenService
{
  public string GenerateToken(AppUser user)
  {
    var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token key from configuration");
    if (tokenKey.Length < 64) throw new Exception("Token key length does not meet the minimum requirements");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
    if (user.UserName == null) throw new Exception("No username found for this user");
    var claims = new List<Claim>
    {
      new (ClaimTypes.NameIdentifier, user.Id),
      new (ClaimTypes.Name, user.UserName)
    };
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    var descriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddDays(7),
      SigningCredentials = creds
    };

    var handler = new JwtSecurityTokenHandler();
    var token = handler.CreateToken(descriptor);
    return handler.WriteToken(token);
  }
}