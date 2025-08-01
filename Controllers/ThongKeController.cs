using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyChiTieu.Data;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ThongKeController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public ThongKeController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: /ThongKe
        public IActionResult Index()
        {
            // TODO: Xây dựng logic để tạo các báo cáo chi tiết
            // Ví dụ: Báo cáo chi tiêu theo từng danh mục trong năm,
            // so sánh chi tiêu giữa các tháng, v.v.
            return View();
        }
    }
}