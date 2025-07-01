using Server.Services.Interfaces;

namespace Server.Services;

public class TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger) : BackgroundService
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly ILogger<TokenCleanupService> _logger = logger;
  private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        using var scope = _serviceProvider.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        await tokenService.CleanupExpiredTokensAsync();
        _logger.LogInformation("Token cleanup completed successfully");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred during token cleanup");
      }

      await Task.Delay(_cleanupInterval, stoppingToken);
    }
  }
}