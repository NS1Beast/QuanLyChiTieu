using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class XepHangController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public XepHangController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);

            // 1. Lấy tổng chi tiêu của TẤT CẢ người dùng trong tháng hiện tại và xếp hạng
            var allUsersSpending = await _context.ChiTieus
                .Where(c => c.NgayChi.HasValue && c.NgayChi >= dauThang && c.NgayChi.Value.Date <= homNay)
                .GroupBy(c => c.NguoiDungId)
                .Select(g => new {
                    UserId = g.Key,
                    TotalSpending = g.Sum(c => c.SoTien)
                })
                .OrderByDescending(x => x.TotalSpending)
                .ToListAsync();

            // 2. Lấy Top 3 ẩn danh
            var topSpenders = allUsersSpending
                .Take(3)
                .Select((item, index) => new TopSpenderViewModel
                {
                    Rank = index + 1,
                    TotalAmount = item.TotalSpending
                })
                .ToList();

            // 3. Tìm thứ hạng của người dùng hiện tại
            var currentUserData = allUsersSpending.Select((item, index) => new { item.UserId, item.TotalSpending, Rank = index + 1 })
                                                   .FirstOrDefault(x => x.UserId == userId);

            var viewModel = new XepHangViewModel
            {
                TopSpenders = topSpenders,
                TotalUsersRanked = allUsersSpending.Count,
                CurrentUserRank = currentUserData?.Rank ?? 0,
                CurrentUserSpending = currentUserData?.TotalSpending ?? 0
            };

            return View(viewModel);
        }
    }
}