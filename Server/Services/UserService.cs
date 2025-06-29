using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Services.Interfaces;

namespace Server.Services;

public class UserService(AppDbContext context) : IUserService
{
  private readonly AppDbContext _context = context;


  public async Task CleanupUserData(string userId)
  {
    await _context.Drawings
      .Where(d => d.UserId == userId)
      .ExecuteDeleteAsync();

    await _context.Follows
      .Where(f => f.FollowerId == userId || f.FollowingId == userId)
      .ExecuteDeleteAsync();

    await _context.Upvotes
    .Where(u => u.AppUserId == userId)
    .ExecuteDeleteAsync();

    await _context.Drawings
    .Where(d => d.UserId == userId)
    .ExecuteDeleteAsync();
  }

}
