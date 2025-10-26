using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using UserService.BO.Exceptions;

namespace UserService.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new AppException("Email address is required.");

            // read config from appsettings, allow ENV to override (same approach as JwtService)
            var host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _config["Smtp:Host"];
            var portStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? _config["Smtp:Port"];
            var username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? _config["Smtp:Username"];
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? _config["Smtp:Password"];
            var from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _config["Smtp:From"] ?? username;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr))
                throw new AppException("SMTP configuration is missing.", HttpStatusCode.BadRequest);

            if (!int.TryParse(portStr, out var port))
                throw new AppException("Invalid SMTP port configuration.", HttpStatusCode.BadRequest);

            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(from ?? "no-reply@example.com"));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject ?? string.Empty;
                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody ?? string.Empty };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(username))
                {
                    await client.AuthenticateAsync(username, password).ConfigureAwait(false);
                }

                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: ${ex.Message}");
                throw new AppException("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }
    }
}