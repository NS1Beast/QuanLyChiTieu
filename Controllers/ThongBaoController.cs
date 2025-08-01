using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ThongBaoController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public ThongBaoController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: /ThongBao
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy tất cả thông báo của người dùng, sắp xếp mới nhất lên đầu
            var thongBaos = await _context.ThongBaos
                .Where(t => t.NguoiDungId == userId)
                .OrderByDescending(t => t.NgayGui)
                .ToListAsync();

            return View(thongBaos);
        }
    }
}