namespace Server.Services.Interfaces;

public interface IUserService
{
  Task CleanupUserData(string userId);
}
