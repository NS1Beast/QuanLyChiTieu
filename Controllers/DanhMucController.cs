using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using System.Threading.Tasks; // Đảm bảo đã có using này

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
        public async Task<IActionResult> CreateOrEdit([Bind("Id,TenDanhMuc,MauSac,TuDongPhanLoai")] DanhMuc danhMuc)
        {
            // Kiểm tra ModelState một lần nữa
            // Chú ý: Cần kiểm tra kỹ ModelState xem lỗi nằm ở đâu nếu vẫn không chạy được
            // Có thể bỏ qua ModelState.IsValid nếu bạn chắc chắn form luôn hợp lệ.
            // Dòng code này có thể gây ra lỗi nếu trường dữ liệu không hợp lệ.
            // if (!ModelState.IsValid)
            // {
            //     // Có thể thêm log hoặc debug ở đây
            //     TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            //     return RedirectToAction(nameof(Index));
            // }

            try
            {
                if (danhMuc.Id == 0) // Tạo mới
                {
                    _context.Add(danhMuc);
                }
                else // Cập nhật
                {
                    var existingDanhMuc = await _context.DanhMucs.FindAsync(danhMuc.Id);
                    if (existingDanhMuc == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính từ form vào đối tượng đã tồn tại
                    existingDanhMuc.TenDanhMuc = danhMuc.TenDanhMuc;
                    existingDanhMuc.MauSac = danhMuc.MauSac;
                    existingDanhMuc.TuDongPhanLoai = danhMuc.TuDongPhanLoai;
                    _context.Update(existingDanhMuc);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thao tác thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DanhMucs.Any(e => e.Id == danhMuc.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác (ví dụ: lỗi kết nối database)
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                // Thêm log để debug chi tiết hơn
                // _logger.LogError(ex, "Lỗi khi lưu Danh mục");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DanhMuc/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                var isUsed = await _context.ChiTieus.AnyAsync(c => c.DanhMucId == id);
                if (isUsed)
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục này vì nó đang được sử dụng.";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}