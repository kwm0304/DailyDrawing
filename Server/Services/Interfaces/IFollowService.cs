using Server.DTOs;
using Server.Entities;

namespace Server.Services.Interfaces;

public interface IFollowService
{
  Task<Follow> CreateFollow(AppUser follower, AppUser following);
  Task<bool> DeleteFollow(AppUser follower, AppUser following);
  Task<List<FollowDto>> GetAllFollowersForUser(string userId);
}
