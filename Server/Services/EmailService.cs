using Microsoft.AspNetCore.Identity;
using Server.Data;
using Server.DTOs;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class EmailService : IEmailService
{
  private readonly AppDbContext _context;
  private readonly UserManager<AppUser> _userManager;
  private readonly ILogger<EmailService> _logger;
  private readonly IConfiguration _config;
  public EmailService(AppDbContext context, UserManager<AppUser> userManager, ILogger<EmailService> logger, IConfiguration config)
  {
    _context = context;
    _userManager = userManager;
    _logger = logger;
    _config = config;
  }
  public Task SendEmail(EmailDto dto)
  {
    throw new NotImplementedException();
  }

  public Task SendForgotPasswordEmailAsync(ForgotPasswordDto dto)
  {
    throw new NotImplementedException();
  }


  public Task SendPasswordResetEmailAsync(ResetPasswordLinkDto dto)
  {
    throw new NotImplementedException();
  }

  public Task SendWelcomeEmail(EmailDto dto)
  {
    throw new NotImplementedException();
  }
}
