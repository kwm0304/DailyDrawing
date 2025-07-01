using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class FollowService(AppDbContext context, ILogger<FollowService> logger) : IFollowService
{
  private readonly AppDbContext _context = context;
  private readonly ILogger<FollowService> _logger = logger;


  public async Task<Follow> CreateFollow(AppUser follower, AppUser following)
  {
    var follow = new Follow
    {
      Follower = follower,
      FollowerId = follower.Id,
      Following = following,
      FollowingId = following.Id
    };
    _context.Follows.Add(follow);
    await _context.SaveChangesAsync();
    return follow;
  }

  public async Task<bool> DeleteFollow(AppUser follower, AppUser following)
  {
    var res = await _context.Follows.Where(f => f.Follower == follower && f.Following == following).ExecuteDeleteAsync();
    if (res > 0)
    {
      _logger.LogInformation("User {Follower} is no longer following {Following}", follower.DisplayName, following.DisplayName);
      return true;
    }
    else
    {
      _logger.LogInformation("Failed to delete follow for {Follower} following {Following}", follower.DisplayName, following.DisplayName);
      return false;
    }
  }

  public async Task<List<FollowDto>> GetAllFollowersForUser(string userId)
  {
    List<Follow> followers = await _context.Follows.Where(f => f.FollowingId == userId).ToListAsync();
    return followers.Select(f => new FollowDto
    {
      FollowerId = f.FollowerId,
      FollowingId = f.FollowingId
    }).ToList() ?? [];
  }

}
