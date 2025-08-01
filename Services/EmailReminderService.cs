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
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataBase_DoAnContext>();
                    await dbContext.Database.ExecuteSqlRawAsync("EXEC sp_GuiNhanNhocTheoThoiGian", stoppingToken);
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Log lại lỗi thay vì để ứng dụng sập
                Console.WriteLine($"Lỗi SQL: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}