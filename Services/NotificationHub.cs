using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace QuanLyChiTieu.Services
{
    public class NotificationHub : Hub
    {
        // Ghi đè phương thức này để thêm user vào một group riêng dựa trên UserId
        // Giúp chúng ta có thể gửi thông báo chỉ cho user đó
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }
    }
}