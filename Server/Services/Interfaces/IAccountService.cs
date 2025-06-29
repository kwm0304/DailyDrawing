namespace Server.Services.Interfaces;

public interface IAccountService
{
  Task<bool> DeleteAccount(string userId);
  
}
