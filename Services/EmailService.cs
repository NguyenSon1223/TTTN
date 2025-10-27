using Ecommerce.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Ecommerce.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !toEmail.Contains("@"))
                throw new FormatException($"Email không hợp lệ: {toEmail}");

            var mail = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:From"], "Ecommerce"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(toEmail));

            using var smtp = new SmtpClient(_config["EmailSettings:Host"], int.Parse(_config["EmailSettings:Port"]))
            {
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true,       // 🔹 BẮT BUỘC cho Gmail
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            // 🔹 Bắt buộc khởi tạo TLS thủ công cho một số SMTP client
            smtp.TargetName = "STARTTLS/smtp.gmail.com";

            await smtp.SendMailAsync(mail);
        }
    }
}
