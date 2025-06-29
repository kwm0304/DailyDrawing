using Server.Services.Interfaces;

namespace Server.Services;

public class AccountService : IAccountService
{
  public Task<bool> DeleteAccount(string userId)
  {
    throw new NotImplementedException();
  }

}
