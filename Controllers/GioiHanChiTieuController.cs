using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;
using System.Linq;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class GioiHanChiTieuController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public GioiHanChiTieuController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: GioiHanChiTieu
        // CẬP NHẬT: Sửa lại Action này để tính toán và gửi ViewModel phù hợp
        public async Task<IActionResult> Index()
        {
            if (TempData["SuccessMessage"] != null) ViewBag.SuccessMessage = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.ErrorMessage = TempData["ErrorMessage"];

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1. Lấy danh sách các hạn mức người dùng đã đặt
            var gioiHanList = await _context.GioiHanChiTieus
                .Where(g => g.NguoiDungId == userId)
                .OrderByDescending(g => g.Nam)
                .ThenByDescending(g => g.Thang)
                .ToListAsync();

            var viewModelList = new List<GioiHanChiTieuItemViewModel>();

            // 2. Lặp qua mỗi hạn mức để tính toán tổng chi tiêu tương ứng
            foreach (var gioiHan in gioiHanList)
            {
                var dauThang = new DateTime(gioiHan.Nam, gioiHan.Thang, 1);
                var cuoiThang = dauThang.AddMonths(1).AddDays(-1);

                var tongChiTieuThang = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == userId &&
                                c.NgayChi.HasValue &&
                                c.NgayChi.Value.Date >= dauThang.Date &&
                                c.NgayChi.Value.Date <= cuoiThang.Date)
                    .SumAsync(c => c.SoTien);

                var phanTram = gioiHan.SoTienToiDa > 0 ? (int)Math.Round((tongChiTieuThang / gioiHan.SoTienToiDa) * 100) : 0;

                viewModelList.Add(new GioiHanChiTieuItemViewModel
                {
                    GioiHan = gioiHan,
                    TongChiTieuThang = tongChiTieuThang,
                    PhanTramDaDung = phanTram
                });
            }

            // 3. Trả về danh sách ViewModel đã được tính toán
            return View(viewModelList);
        }

        // POST: GioiHanChiTieu/Set
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Set(GioiHanChiTieuViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra: " + string.Join("; ", errors);
                return RedirectToAction(nameof(Index));
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingLimit = await _context.GioiHanChiTieus
                .FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.Thang == model.Thang && g.Nam == model.Nam);

            if (existingLimit != null)
            {
                existingLimit.SoTienToiDa = model.SoTienToiDa;
                _context.Update(existingLimit);
            }
            else
            {
                var newLimit = new GioiHanChiTieu
                {
                    SoTienToiDa = model.SoTienToiDa,
                    Thang = model.Thang,
                    Nam = model.Nam,
                    NguoiDungId = userId
                };
                _context.Add(newLimit);
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thiết lập hạn mức cho tháng {model.Thang}/{model.Nam} thành công!";

            return RedirectToAction(nameof(Index));
        }

        // POST: GioiHanChiTieu/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var gioiHan = await _context.GioiHanChiTieus
                .FirstOrDefaultAsync(g => g.Id == id && g.NguoiDungId == userId);

            if (gioiHan != null)
            {
                _context.GioiHanChiTieus.Remove(gioiHan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa hạn mức của tháng {gioiHan.Thang}/{gioiHan.Nam}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy hạn mức để xóa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}