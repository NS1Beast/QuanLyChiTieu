using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

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
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var gioiHanList = await _context.GioiHanChiTieus
                .Where(g => g.NguoiDungId == userId)
                .OrderByDescending(g => g.Nam)
                .ThenByDescending(g => g.Thang)
                .ToListAsync();
            return View(gioiHanList);
        }

        // POST: GioiHanChiTieu/Set
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Set(GioiHanChiTieuViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Kiểm tra xem đã có hạn mức cho tháng/năm này chưa
                var existingLimit = await _context.GioiHanChiTieus
                    .FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.Thang == model.Thang && g.Nam == model.Nam);

                if (existingLimit != null) // Nếu có -> Cập nhật
                {
                    existingLimit.SoTienToiDa = model.SoTienToiDa;
                    _context.Update(existingLimit);
                }
                else // Nếu chưa có -> Tạo mới
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
            }
            // Nếu model không hợp lệ, TempData có thể được dùng để báo lỗi
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}