using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
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

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
            var bayNgayTruoc = homNay.AddDays(-6);

            // Lấy chi tiêu trong tháng hiện tại (tính đến hôm nay)
            var chiTieuTrongThang = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi >= dauThang && c.NgayChi <= homNay)
                .Include(c => c.DanhMuc)
                .ToListAsync();

            // Tính toán mục chi nhiều nhất
            var danhMucChiNhieuNhat = chiTieuTrongThang
                .GroupBy(c => c.DanhMuc.TenDanhMuc)
                .Select(g => new { Ten = g.Key, TongTien = g.Sum(c => c.SoTien) })
                .OrderByDescending(g => g.TongTien)
                .FirstOrDefault();

            // Lấy dữ liệu cho biểu đồ 7 ngày
            var chiTieu7Ngay = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi.Value.Date >= bayNgayTruoc && c.NgayChi.Value.Date <= homNay)
                .GroupBy(c => c.NgayChi.Value.Date)
                .Select(g => new { Ngay = g.Key, TongTien = g.Sum(c => c.SoTien) })
                .ToDictionaryAsync(x => x.Ngay, x => x.TongTien);

            var labels7Ngay = Enumerable.Range(0, 7).Select(i => homNay.AddDays(-i).ToString("dd/MM")).Reverse().ToList();
            var data7Ngay = labels7Ngay.Select(ngay => {
                DateTime.TryParseExact(ngay + "/" + homNay.Year, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d);
                return chiTieu7Ngay.ContainsKey(d.Date) ? chiTieu7Ngay[d.Date] : 0;
            }).ToList();

            // Lấy dữ liệu cho biểu đồ danh mục
            var chiTieuTheoDanhMuc = chiTieuTrongThang
                .GroupBy(c => c.DanhMuc.TenDanhMuc)
                .Select(g => new {
                    TenDanhMuc = g.Key,
                    TongTien = g.Sum(c => c.SoTien),
                    MauSac = g.First().DanhMuc.MauSac
                })
                .ToList();

            // Lấy hạn mức của tháng hiện tại
            var gioiHan = await _context.GioiHanChiTieus
                .FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.Thang == homNay.Month && g.Nam == homNay.Year);

            // === SỬA LOGIC AI TẠI ĐÂY ===
            // AI Gợi ý: Dựa trên chi tiêu tháng này để đề xuất cho tháng sau
            var soNgayTrongThangSau = DateTime.DaysInMonth(homNay.Year, homNay.Month % 12 + 1);
            var soNgayDaChiTieu = homNay.Day;

            var deXuatAI = chiTieuTrongThang
                .GroupBy(c => c.DanhMuc.TenDanhMuc)
                .Select(g => new
                {
                    DanhMuc = g.Key,
                    // (Tổng tiền / số ngày đã qua) * số ngày tháng sau
                    DeXuat = (g.Sum(c => c.SoTien) / soNgayDaChiTieu) * soNgayTrongThangSau
                })
                .ToDictionary(x => x.DanhMuc, x => x.DeXuat);

            // Tạo ViewModel
            var viewModel = new DashboardViewModel
            {
                TongChiThangNay = chiTieuTrongThang.Sum(c => c.SoTien),
                ChiTieuHomNay = chiTieuTrongThang.Where(c => c.NgayChi.HasValue && c.NgayChi.Value.Date == homNay).Sum(c => c.SoTien),
                SoGiaoDichThangNay = chiTieuTrongThang.Count,
                DanhMucChiNhieuNhat = danhMucChiNhieuNhat?.Ten ?? "N/A",
                HanMucThangNay = gioiHan?.SoTienToiDa ?? 0,
                DeXuatAI = deXuatAI,

                JsonChiTieu7NgayGanNhat = JsonSerializer.Serialize(new { labels = labels7Ngay, data = data7Ngay }),
                JsonChiTieuTheoDanhMuc = JsonSerializer.Serialize(new
                {
                    labels = chiTieuTheoDanhMuc.Select(x => x.TenDanhMuc),
                    data = chiTieuTheoDanhMuc.Select(x => x.TongTien),
                    colors = chiTieuTheoDanhMuc.Select(x => x.MauSac)
                }),

                GiaoDichGanDay = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == userId)
                    .OrderByDescending(c => c.NgayChi)
                    .Take(5)
                    .Include(c => c.DanhMuc)
                    .ToListAsync(),

                TatCaDanhMuc = await _context.DanhMucs.ToListAsync()
            };

            return View(viewModel);
        }
    }
}