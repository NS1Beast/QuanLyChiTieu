using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class DanhMucController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public DanhMucController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: DanhMuc
        public async Task<IActionResult> Index()
        {
            var danhMucs = await _context.DanhMucs.ToListAsync();
            return View(danhMucs);
        }

        // POST: DanhMuc/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CẬP NHẬT: Thêm "TuDongPhanLoai" vào danh sách Bind
        public async Task<IActionResult> CreateOrEdit(int id, [Bind("Id,TenDanhMuc,MauSac,TuDongPhanLoai")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                if (id == 0) // Tạo mới
                {
                    _context.Add(danhMuc);
                    await _context.SaveChangesAsync();
                }
                else // Cập nhật
                {
                    if (id != danhMuc.Id) return NotFound();
                    try
                    {
                        _context.Update(danhMuc);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.DanhMucs.Any(e => e.Id == danhMuc.Id)) return NotFound();
                        else throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Nếu không hợp lệ, quay lại trang Index
            return RedirectToAction(nameof(Index));
        }

        // POST: DanhMuc/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                // Thêm logic kiểm tra xem danh mục có đang được sử dụng không trước khi xóa
                var isUsed = await _context.ChiTieus.AnyAsync(c => c.DanhMucId == id);
                if (isUsed)
                {
                    // Tùy chọn: Thêm thông báo lỗi vào TempData để hiển thị trên View
                    TempData["ErrorMessage"] = "Không thể xóa danh mục này vì nó đang được sử dụng.";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}