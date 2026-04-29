using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace WebMusicPlayer.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            // Получаем настройки
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:Password"];


            System.Diagnostics.Debug.WriteLine($"SMTP Host: [{host}]");
            System.Diagnostics.Debug.WriteLine($"SMTP Username: [{username}]");
            System.Diagnostics.Debug.WriteLine($"SMTP Password: [{password}]");
            // Проверка на пустые значения
            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("SMTP Host не настроен в appsettings.json");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("SMTP Username не настроен в appsettings.json");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("SMTP Password не настроен в appsettings.json");

            if (!int.TryParse(portStr, out var port))
                port = 587;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }

        public string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}