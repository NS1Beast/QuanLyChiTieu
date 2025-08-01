using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;

public class EmailReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public EmailReminderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataBase_DoAnContext>();

                // Gọi Stored Procedure
                await dbContext.Database.ExecuteSqlRawAsync("EXEC sp_GuiNhanNhocTheoThoiGian", stoppingToken);
            }

            // Chờ 1 ngày trước khi chạy lại
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}