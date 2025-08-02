using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public DashboardController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: /Dashboard - Tải dữ liệu chính cho trang
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
            var bayNgayTruoc = homNay.AddDays(-6);

            var chiTieuTrongThang = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi >= dauThang && c.NgayChi <= homNay.AddDays(1).AddTicks(-1))
                .Include(c => c.DanhMuc)
                .ToListAsync();

            var danhMucChiNhieuNhat = chiTieuTrongThang
                .GroupBy(c => c.DanhMuc.TenDanhMuc)
                .Select(g => new { Ten = g.Key, TongTien = g.Sum(c => c.SoTien) })
                .OrderByDescending(g => g.TongTien)
                .FirstOrDefault();

            var chiTieu7Ngay = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi.Value.Date >= bayNgayTruoc && c.NgayChi.Value.Date <= homNay)
                .GroupBy(c => c.NgayChi.Value.Date)
                .Select(g => new { Ngay = g.Key, TongTien = g.Sum(c => c.SoTien) })
                .ToDictionaryAsync(x => x.Ngay, x => x.TongTien);

            var labels7Ngay = Enumerable.Range(0, 7).Select(i => homNay.AddDays(-i).ToString("dd/MM")).Reverse().ToList();
            var data7Ngay = labels7Ngay.Select(ngay => { DateTime.TryParseExact(ngay + "/" + homNay.Year, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d); return chiTieu7Ngay.ContainsKey(d.Date) ? chiTieu7Ngay[d.Date] : 0; }).ToList();

            var chiTieuTheoDanhMuc = chiTieuTrongThang
                .GroupBy(c => c.DanhMuc.TenDanhMuc)
                .Select(g => new { TenDanhMuc = g.Key, TongTien = g.Sum(c => c.SoTien), MauSac = g.First().DanhMuc.MauSac })
                .ToList();

            var gioiHan = await _context.GioiHanChiTieus
                .FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.Thang == homNay.Month && g.Nam == homNay.Year);

            var viewModel = new DashboardViewModel
            {
                TongChiThangNay = chiTieuTrongThang.Sum(c => c.SoTien),
                ChiTieuHomNay = chiTieuTrongThang.Where(c => c.NgayChi.HasValue && c.NgayChi.Value.Date == homNay).Sum(c => c.SoTien),
                SoGiaoDichThangNay = chiTieuTrongThang.Count,
                DanhMucChiNhieuNhat = danhMucChiNhieuNhat?.Ten ?? "N/A",
                HanMucThangNay = gioiHan?.SoTienToiDa ?? 0,
                JsonChiTieu7NgayGanNhat = JsonSerializer.Serialize(new { labels = labels7Ngay, data = data7Ngay }),
                JsonChiTieuTheoDanhMuc = JsonSerializer.Serialize(new { labels = chiTieuTheoDanhMuc.Select(x => x.TenDanhMuc), data = chiTieuTheoDanhMuc.Select(x => x.TongTien), colors = chiTieuTheoDanhMuc.Select(x => x.MauSac) }),
                GiaoDichGanDay = await _context.ChiTieus.Where(c => c.NguoiDungId == userId).OrderByDescending(c => c.NgayChi).Take(5).Include(c => c.DanhMuc).ToListAsync(),
                TatCaDanhMuc = await _context.DanhMucs.ToListAsync()
            };

            return View(viewModel);
        }

        // GET: /Dashboard/GetAISuggestion - Xử lý yêu cầu AI qua AJAX
        [HttpGet]
        public async Task<IActionResult> GetAISuggestion()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);

            var chiTieuTrongThang = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi >= dauThang && c.NgayChi <= homNay)
                .Include(c => c.DanhMuc)
                .ToListAsync();

            if (!chiTieuTrongThang.Any())
            {
                return Json(new { success = false, message = "Không có dữ liệu chi tiêu trong tháng này để phân tích." });
            }

            var soNgayTrongThangSau = DateTime.DaysInMonth(homNay.Year, (homNay.Month % 12) + 1);
            var soNgayDaChiTieu = homNay.Day;
            if (soNgayDaChiTieu == 0) soNgayDaChiTieu = 1; // Tránh chia cho 0

            var deXuatAI = chiTieuTrongThang
                .GroupBy(c => c.DanhMuc.TenDanhMuc)
                .Select(g => new
                {
                    DanhMuc = g.Key,
                    DeXuat = (g.Sum(c => c.SoTien) / soNgayDaChiTieu) * soNgayTrongThangSau
                })
                .ToDictionary(x => x.DanhMuc, x => x.DeXuat);

            var tenThangHienTai = $"tháng {homNay.Month}";
            var tenThangSau = $"tháng {(homNay.Month % 12) + 1}";
            var tongChiHienTai = chiTieuTrongThang.Sum(c => c.SoTien);
            var trungBinhNgay = tongChiHienTai / soNgayDaChiTieu;

            string explanation = $"Chào bạn, tôi là trợ lý AI. Sau khi xem xét {chiTieuTrongThang.Count} khoản chi của bạn trong {soNgayDaChiTieu} ngày đã qua của {tenThangHienTai}, " +
                                 $"tôi nhận thấy trung bình mỗi ngày bạn đang chi tiêu khoảng {trungBinhNgay:N0} ₫. " +
                                 $"Dựa trên tốc độ này, tôi dự phóng ngân sách cho {soNgayTrongThangSau} ngày của {tenThangSau} như sau:";

            return Json(new
            {
                success = true,
                explanation,
                suggestions = deXuatAI.Select(kvp => new { category = kvp.Key, amount = kvp.Value }),
                isEarlyMonth = homNay.Day <= 5
            });
        }
    }
}