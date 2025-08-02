using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

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
        public async Task<IActionResult> Index(int? year)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy danh sách các năm có chi tiêu để làm bộ lọc
            var availableYears = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue)
                .Select(c => c.NgayChi.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Any())
            {
                availableYears.Add(DateTime.Now.Year);
            }

            // Xác định năm được chọn (mặc định là năm hiện tại)
            int selectedYear = year ?? DateTime.Now.Year;

            // Lấy tất cả chi tiêu trong năm được chọn
            var expensesInYear = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi.Value.Year == selectedYear)
                .Include(c => c.DanhMuc)
                .ToListAsync();

            // 1. Chuẩn bị dữ liệu cho biểu đồ đường (chi tiêu hàng tháng)
            var monthlySpending = expensesInYear
                .GroupBy(c => c.NgayChi.Value.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(c => c.SoTien) })
                .ToDictionary(x => x.Month, x => x.Total);

            var monthlyData = new decimal[12];
            for (int i = 1; i <= 12; i++)
            {
                monthlyData[i - 1] = monthlySpending.ContainsKey(i) ? monthlySpending[i] : 0;
            }

            // 2. Chuẩn bị dữ liệu cho bảng phân tích (chi tiêu theo danh mục)
            var totalYearlySpending = expensesInYear.Sum(c => c.SoTien);
            var categoryBreakdown = expensesInYear
                .GroupBy(c => new { c.DanhMuc.TenDanhMuc, c.DanhMuc.MauSac })
                .Select(g => new CategorySpending
                {
                    TenDanhMuc = g.Key.TenDanhMuc,
                    MauSac = g.Key.MauSac,
                    TongTien = g.Sum(c => c.SoTien),
                    TyTrong = totalYearlySpending > 0 ? (double)(g.Sum(c => c.SoTien) / totalYearlySpending) * 100 : 0
                })
                .OrderByDescending(x => x.TongTien)
                .ToList();

            // 3. Tạo ViewModel
            var viewModel = new ThongKeViewModel
            {
                SelectedYear = selectedYear,
                YearList = new SelectList(availableYears),
                JsonMonthlySpending = JsonSerializer.Serialize(monthlyData),
                CategoryBreakdown = categoryBreakdown,
                TotalYearlySpending = totalYearlySpending
            };

            return View(viewModel);
        }
    }
}