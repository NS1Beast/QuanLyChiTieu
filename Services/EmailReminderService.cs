using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;

namespace QuanLyChiTieu.Services
{
    public class EmailReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailReminderService> _logger;

        public EmailReminderService(IServiceProvider serviceProvider, ILogger<EmailReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DataBase_DoAnContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        // Lấy danh sách người dùng cần nhận nhắc nhở hôm nay
                        var usersToRemind = await dbContext.NguoiDungs
                            .Where(u => u.NhanEmailNhacNho == true && u.GioNhanNhac.HasValue && u.GioNhanNhac.Value.Hour == DateTime.Now.Hour)
                            .ToListAsync(stoppingToken);

                        foreach (var user in usersToRemind)
                        {
                            var totalSpentToday = await dbContext.ChiTieus
                                .Where(c => c.NguoiDungId == user.Id && c.NgayChi.HasValue && c.NgayChi.Value.Date == DateTime.Today)
                                .SumAsync(c => c.SoTien, stoppingToken);

                            // Gửi email
                            var subject = "📧 Nhắc nhở chi tiêu hàng ngày của bạn";
                            var message = $"<p>Chào {user.HoTen ?? "bạn"},</p>" +
                                          $"<p>Hôm nay bạn đã chi tiêu tổng cộng: <strong>{totalSpentToday:N0} ₫</strong>.</p>" +
                                          $"<p>Hãy tiếp tục theo dõi để đạt được mục tiêu tài chính của mình nhé!</p>";

                            await emailService.SendEmailAsync(user.Email, subject, message);

                            // Ghi lại lịch sử sau khi gửi thành công
                            var history = new Models.LichSuNhanNhac
                            {
                                TieuDe = subject,
                                NoiDung = message,
                                LoaiNhanNhac = "HangNgay",
                                NguoiDungId = user.Id
                            };
                            dbContext.LichSuNhanNhacs.Add(history);
                        }
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong EmailReminderService.");
                }

                // Chờ 1 giờ rồi chạy lại
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}