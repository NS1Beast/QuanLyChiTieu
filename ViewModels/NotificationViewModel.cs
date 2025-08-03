using QuanLyChiTieu.Models;
using System.Collections.Generic;

namespace QuanLyChiTieu.ViewModels
{
    public class NotificationViewModel
    {
        public List<ThongBao> RecentNotifications { get; set; } = new List<ThongBao>();
        public int UnreadCount { get; set; }
    }
}