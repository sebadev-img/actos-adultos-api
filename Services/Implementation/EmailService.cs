using ActosAdultosAPI.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace ActosAdultosAPI.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string htmlBody)
        {
            var host = _configuration["Smtp:Host"]
                ?? throw new InvalidOperationException(
                    "SMTP Host no configurado.");

            var username = _configuration["Smtp:Username"]
                ?? throw new InvalidOperationException(
                    "SMTP Username no configurado.");

            var password = _configuration["Smtp:Password"]
                ?? throw new InvalidOperationException(
                    "SMTP Password no configurado.");

            var fromEmail = _configuration["Smtp:FromEmail"]
                ?? throw new InvalidOperationException(
                    "SMTP FromEmail no configurado.");

            var port = _configuration.GetValue<int>(
                "Smtp:Port", 587);

            var enableSsl = _configuration.GetValue<bool>(
                "Smtp:EnableSsl", true);

            var fromName = _configuration["Smtp:FromName"]
                ?? "Mi Web API";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    username, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
        }
    }
}
