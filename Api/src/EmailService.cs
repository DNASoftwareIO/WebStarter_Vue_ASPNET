using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

public class SmtpSettings
{
  public bool SendingEnabled { get; set; } = false;
  public string From { get; set; } = string.Empty;
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; }
  public string Password { get; set; } = string.Empty;
}


public class EmailService
{
  private readonly SmtpSettings _settings;
  private readonly IConfiguration _configuration;
  private readonly UserManager<User> _userManager;

  public EmailService(IOptions<SmtpSettings> settings, IConfiguration configuration, UserManager<User> userManager)
  {
    _configuration = configuration;
    _userManager = userManager;
    _settings = settings.Value;
  }

  public async Task SendEmail(string to, string subject, string message)
  {
    if (!_settings.SendingEnabled)
    {
      // Writing to console to allow easier testing locally
      Console.WriteLine(to);
      Console.WriteLine(subject);
      Console.WriteLine(message);
      return;
    }

    var email = new MimeMessage();
    email.From.Add(MailboxAddress.Parse(_settings.From));
    email.To.Add(MailboxAddress.Parse(to));
    email.Subject = subject;
    email.Body = new TextPart(TextFormat.Html) { Text = message };

    using var smtp = new SmtpClient();

    smtp.Connect(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
    smtp.Authenticate(_settings.From, _settings.Password);

    var result = await smtp.SendAsync(email);
    smtp.Disconnect(true);
  }

  public async Task SendEmailConfirmationToken(User user)
  {
    if (user.Email == null) return;

    // TODO I think this could generate tokens that don't work if there are multiple instances of this app. 
    // We need to share a machine key, see here https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-3.1
    // or manage tokens ourselves
    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    // TODO load from template
    var subject = "Email verification";
    StringBuilder message = new();
    message.AppendFormat("Hi {0},\n Click the link below to verify your email.\n", user.UserName);
    message.AppendFormat("<a href=\"{0}\">Verify Email Address</a>", _configuration["WebClientUrl"] + "/confirm-email?userId=" + user.Id.ToString() + "&token=" + token);

    await SendEmail(user.Email, subject, message.ToString());
  }

  public async Task SendPasswordResetToken(User user)
  {
    if (user.Email == null) return;

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    // TODO load from template 
    var subject = "Reset Password";
    StringBuilder message = new();
    message.AppendFormat(string.Format("Hi {0},\n Click the link below to reset your password.\n", user.UserName));
    message.AppendFormat(string.Format("<a href=\"{0}\" target=\"_blank\">Reset Password</a>", _configuration["WebClientUrl"] + "/reset-password?email=" + user.Email + "&token=" + token));

    await SendEmail(user.Email, subject, message.ToString());
  }
}
