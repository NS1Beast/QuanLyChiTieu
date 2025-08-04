using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class LichChiTieuController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public LichChiTieuController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var scheduledExpenses = await _context.ChiTieuTheoLiches
                .Where(s => s.NguoiDungId == userId)
                .Include(s => s.DanhMuc)
                .OrderBy(s => s.NgayThucHien)
                .ToListAsync();

            if (TempData["SuccessMessage"] != null) ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View(scheduledExpenses);
        }

        public IActionResult Create()
        {
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenChiTieu,SoTien,GhiChu,NgayThucHien,DanhMucId")] ChiTieuTheoLich scheduledExpense)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            scheduledExpense.NguoiDungId = userId;
            scheduledExpense.HoatDong = true;

            // SỬA LỖI TẠI ĐÂY: Bỏ .HasValue và .Value
            var todayAsDateOnly = DateOnly.FromDateTime(DateTime.Today);
            if (scheduledExpense.NgayThucHien < todayAsDateOnly)
            {
                ModelState.AddModelError("NgayThucHien", "Không thể đặt lịch chi tiêu cho một ngày trong quá khứ.");
            }

            var targetMonth = scheduledExpense.NgayThucHien.Month;
            var targetYear = scheduledExpense.NgayThucHien.Year;
            var targetMonthLimit = await _context.GioiHanChiTieus
                .AnyAsync(g => g.NguoiDungId == userId && g.Thang == targetMonth && g.Nam == targetYear);

            if (!targetMonthLimit)
            {
                ModelState.AddModelError(string.Empty, $"Bạn phải đặt hạn mức cho tháng {targetMonth}/{targetYear} trước khi tạo lịch chi tiêu.");
            }

            ModelState.Remove("NguoiDung");
            ModelState.Remove("DanhMuc");

            if (ModelState.IsValid)
            {
                _context.Add(scheduledExpense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo lịch chi tiêu mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", scheduledExpense.DanhMucId);
            return View(scheduledExpense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var scheduledExpense = await _context.ChiTieuTheoLiches.FirstOrDefaultAsync(s => s.Id == id && s.NguoiDungId == userId);
            if (scheduledExpense != null)
            {
                _context.ChiTieuTheoLiches.Remove(scheduledExpense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa lịch chi tiêu thành công.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}