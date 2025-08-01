using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ChiTieuController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public ChiTieuController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: ChiTieu
        public async Task<IActionResult> Index(string danhMucFilter, string searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query = _context.ChiTieus
                .Where(c => c.NguoiDungId == userId)
                .Include(c => c.DanhMuc)
                .OrderByDescending(c => c.NgayChi)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.TenChiTieu.Contains(searchString) || (s.GhiChu != null && s.GhiChu.Contains(searchString)));
            }

            if (!String.IsNullOrEmpty(danhMucFilter))
            {
                query = query.Where(x => x.DanhMuc.TenDanhMuc == danhMucFilter);
            }

            if (tuNgay.HasValue)
            {
                // Tinh chỉnh: Thêm kiểm tra HasValue để truy vấn rõ ràng hơn
                query = query.Where(x => x.NgayChi.HasValue && x.NgayChi.Value.Date >= tuNgay.Value.Date);
            }
            if (denNgay.HasValue)
            {
                // Tinh chỉnh: Thêm kiểm tra HasValue và lấy đến hết ngày
                query = query.Where(x => x.NgayChi.HasValue && x.NgayChi.Value.Date <= denNgay.Value.Date);
            }

            var chiTieuList = await query.ToListAsync();

            var viewModel = new ChiTieuIndexViewModel
            {
                ChiTieuList = chiTieuList,
                DanhMucList = new SelectList(await _context.DanhMucs.Select(d => d.TenDanhMuc).Distinct().ToListAsync()),
                TongChiTieu = chiTieuList.Sum(c => c.SoTien),
                SearchString = searchString,
                DanhMucFilter = danhMucFilter,
                TuNgay = tuNgay,
                DenNgay = denNgay
            };

            return View(viewModel);
        }

        // GET: ChiTieu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var chiTieu = await _context.ChiTieus
                .Include(c => c.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id && m.NguoiDungId == userId);
            if (chiTieu == null) return NotFound();
            return View(chiTieu);
        }

        // GET: ChiTieu/Create
        public IActionResult Create()
        {
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc");
            return View();
        }

        // POST: ChiTieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenChiTieu,SoTien,NgayChi,GhiChu,DanhMucId")] ChiTieu chiTieu)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            chiTieu.NguoiDungId = userId;

            ModelState.Remove("NguoiDung");
            ModelState.Remove("DanhMuc");

            if (ModelState.IsValid)
            {
                _context.Add(chiTieu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", chiTieu.DanhMucId);
            return View(chiTieu);
        }

        // GET: ChiTieu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var chiTieu = await _context.ChiTieus.FindAsync(id);
            if (chiTieu == null || chiTieu.NguoiDungId != userId) return NotFound();
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", chiTieu.DanhMucId);
            return View(chiTieu);
        }

        // POST: ChiTieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenChiTieu,SoTien,NgayChi,GhiChu,DanhMucId")] ChiTieu chiTieu)
        {
            if (id != chiTieu.Id) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ModelState.Remove("NguoiDung");
            ModelState.Remove("DanhMuc");

            if (ModelState.IsValid)
            {
                // Kiểm tra quyền sở hữu trước khi cập nhật
                var originalChiTieu = await _context.ChiTieus.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.NguoiDungId == userId);
                if (originalChiTieu == null) return NotFound();

                // Gán lại NguoiDungId để đảm bảo an toàn
                chiTieu.NguoiDungId = userId;

                try
                {
                    _context.Update(chiTieu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTieuExists(chiTieu.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", chiTieu.DanhMucId);
            return View(chiTieu);
        }

        // GET: ChiTieu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var chiTieu = await _context.ChiTieus
                .Include(c => c.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id && m.NguoiDungId == userId);
            if (chiTieu == null) return NotFound();
            return View(chiTieu);
        }

        // POST: ChiTieu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var chiTieu = await _context.ChiTieus.FirstOrDefaultAsync(c => c.Id == id && c.NguoiDungId == userId);

            if (chiTieu != null)
            {
                _context.ChiTieus.Remove(chiTieu);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // --- ĐÃ XÓA PHẦN BỊ TRÙNG LẶP VÀ GIỮ LẠI PHIÊN BẢN ĐÚNG ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuggestCategory(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false });
            }

            var keyword = await _context.TuKhoaDanhMucs
                .FirstOrDefaultAsync(k => query.ToLower().Contains(k.TuKhoa.ToLower()));

            if (keyword != null)
            {
                return Json(new { success = true, categoryId = keyword.DanhMucId });
            }

            return Json(new { success = false });
        }

        private bool ChiTieuExists(int id)
        {
            return _context.ChiTieus.Any(e => e.Id == id);
        }
    }
}