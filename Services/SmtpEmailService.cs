using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // Thêm using này
using MimeKit;

namespace QuanLyChiTieu.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<SmtpEmailService> _logger;

        // CẬP NHẬT: Tiêm IOptions<MailSettings> vào constructor để đọc cấu hình
        public SmtpEmailService(IOptions<MailSettings> mailSettings, ILogger<SmtpEmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailMessage = new MimeMessage();

                // CẬP NHẬT: Sử dụng thông tin từ _mailSettings
                emailMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // CẬP NHẬT: Sử dụng thông tin từ _mailSettings
                    await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đến {ToEmail}", email);
            }
        }
    }
}