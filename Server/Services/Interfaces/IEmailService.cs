using Server.DTOs;

namespace Server.Services.Interfaces;

public interface IEmailService
{
  Task SendPasswordResetEmailAsync(ResetPasswordLinkDto dto);
  Task SendForgotPasswordEmailAsync(ForgotPasswordDto dto);
  Task SendEmail(EmailDto dto);
  Task SendWelcomeEmail(EmailDto dto);
}
