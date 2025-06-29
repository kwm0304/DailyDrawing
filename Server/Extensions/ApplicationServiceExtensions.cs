using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Mappers;
using Server.Services;
using Server.Services.Interfaces;

namespace Server.Extensions;

public static class ApplicationServiceExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
  {
    services.AddControllers();
    services.AddDbContext<AppDbContext>(opt =>
    {
      opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    });
    services.AddCors(options =>
    {
      options.AddPolicy("CorsPolicy", builder =>
      {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
      });
    });
    services.AddScoped<IBadgeAwardingService, BadgeAwardingService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IBadgeService, BadgeService>();
    services.AddScoped<ICommentService, CommentService>();
    services.AddScoped<IFollowService, FollowService>();
    services.AddScoped<IS3Service, S3Service>();
    services.AddScoped<IDrawingService, DrawingService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<Mapper>();
    return services;
  }
}