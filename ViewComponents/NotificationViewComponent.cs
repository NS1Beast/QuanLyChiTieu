using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.ViewModels; // Thêm using này
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyChiTieu.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly DataBase_DoAnContext _context;

        public NotificationViewComponent(DataBase_DoAnContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new NotificationViewModel();
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier));

                // Lấy 5 thông báo gần nhất
                viewModel.RecentNotifications = await _context.ThongBaos
                    .Where(t => t.NguoiDungId == userId)
                    .OrderByDescending(t => t.NgayGui)
                    .Take(5)
                    .ToListAsync();

                // Đếm số thông báo chưa đọc
                viewModel.UnreadCount = await _context.ThongBaos
                    .CountAsync(t => t.NguoiDungId == userId && t.DaDoc == false);
            }
            return View(viewModel);
        }
    }
}