namespace Server.Config;

public class EmailConfig
{
  public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ReplyToEmail { get; set; } = string.Empty;
    public string ReplyToName { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30000;
    public bool EnableLogging { get; set; } = true;
}
