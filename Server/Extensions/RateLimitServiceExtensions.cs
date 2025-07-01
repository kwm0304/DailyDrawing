using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Server.Extensions;

public static class RateLimitServiceExtensions
{
  public static IServiceCollection AddRateLimiter(this IServiceCollection services, IConfiguration config)
  {
    services.AddRateLimiter(opt =>
    {
      opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
          factory: partition => new FixedWindowRateLimiterOptions
          {
            AutoReplenishment = true,
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1)
          })
      );
      opt.AddFixedWindowLimiter("AuthPolicy", options =>
      {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
      });
      opt.AddFixedWindowLimiter("PasswordResetPolicy", options =>
        {
          options.PermitLimit = 3;
          options.Window = TimeSpan.FromMinutes(15);
          options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
          options.QueueLimit = 0;
        });
      opt.AddFixedWindowLimiter("TokenRefreshPolicy", options =>
      {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 1;
      });
      opt.AddFixedWindowLimiter("ApiPolicy", options =>
      {
        options.PermitLimit = 1000;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 10;
      });
      opt.OnRejected = async (context, token) =>
      {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
          context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }
        var response = new
        {
          error = "Too many requests",
          message = "Rate limit exceeded. Please try again later.",
          retryAfter = retryAfter.TotalSeconds
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
      };
    });
    return services;
  }
}