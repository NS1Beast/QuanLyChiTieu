using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyChiTieu.Services
{
    public class ScheduledTransactionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledTransactionService> _logger;

        public ScheduledTransactionService(IServiceProvider serviceProvider, ILogger<ScheduledTransactionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // ...
                await ProcessScheduledExpenses(stoppingToken);
                // ...
            }
        }

        private async Task ProcessScheduledExpenses(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataBase_DoAnContext>();
                var today = DateTime.Today;

                // SỬA LỖI TẠI ĐÂY: Bỏ .HasValue và .Value
                var schedulesToRun = await context.ChiTieuTheoLiches
                    .Where(s => s.HoatDong == true && s.NgayThucHien == DateOnly.FromDateTime(today))
                    .ToListAsync(stoppingToken);

                if (!schedulesToRun.Any())
                {
                    _logger.LogInformation("No scheduled expenses to run today.");
                    return;
                }

                foreach (var schedule in schedulesToRun)
                {
                    var newExpense = new ChiTieu
                    {
                        TenChiTieu = schedule.TenChiTieu,
                        SoTien = schedule.SoTien,
                        NgayChi = today,
                        GhiChu = $"Chi tiêu tự động từ lịch: {schedule.GhiChu}",
                        NguoiDungId = schedule.NguoiDungId,
                        DanhMucId = schedule.DanhMucId
                    };
                    context.ChiTieus.Add(newExpense);

                    schedule.HoatDong = false;
                    schedule.LanThucHienCuoi = DateOnly.FromDateTime(today);
                    context.Update(schedule);

                    _logger.LogInformation($"Executed scheduled expense '{schedule.TenChiTieu}' for user {schedule.NguoiDungId}.");
                }
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}