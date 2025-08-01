using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging; // Thêm using cho ILogger

namespace QuanLyChiTieu.Services
{
    // Lớp này dùng để gửi email thật sự
    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;

        // Sử dụng ILogger để ghi lại lỗi tốt hơn
        public SmtpEmailService(ILogger<SmtpEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailMessage = new MimeMessage();

                // SỬA LỖI 1: Email người gửi phải khớp với email xác thực
                emailMessage.From.Add(new MailboxAddress("Quản Lý Chi Tiêu", "fvgfv123nam@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // SỬA LỖI 2: Phải dùng SMTP server của Gmail
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    // Thông tin xác thực của bạn (đã đúng)
                    await client.AuthenticateAsync("fvgfv123nam@gmail.com", "wqve qbwc nojg fzyc");

                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi chi tiết thay vì chỉ in ra Console
                _logger.LogError(ex, "Lỗi khi gửi email đến {ToEmail}", email);
            }
        }
    }
}