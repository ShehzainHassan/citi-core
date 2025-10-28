using citi_core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace citi_core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendOTPEmailAsync(string toEmail, string otpCode)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var emailPassword = _configuration["EmailSettings:EmailPassword"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Your OTP Code",
                Body = $"Your One-Time Password (OTP) is: {otpCode}\n\nThis code is valid for 10 minutes.",
                IsBodyHtml = false
            };

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, emailPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }
        public async Task SendAccountLockoutEmailAsync(string toEmail, string userName)
        {
            string body = $@"
Hello {userName},

Your account has been temporarily locked due to multiple failed login attempts.

This lockout is for your protection. If this was not you, please reach out to support or reset your password.

Regards,
Security Team
";

            await SendEmailAsync(toEmail, "Account Locked - Security Alert", body);
        }
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var emailPassword = _configuration["EmailSettings:EmailPassword"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

            using var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, emailPassword),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}