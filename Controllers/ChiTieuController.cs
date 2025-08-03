using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR; // Thêm using SignalR
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services; // Thêm using Services
using QuanLyChiTieu.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ChiTieuController : Controller
    {
        private readonly DataBase_DoAnContext _context;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext;

        // CẬP NHẬT CONSTRUCTOR: Tiêm thêm IEmailService và IHubContext
        public ChiTieuController(DataBase_DoAnContext context, IEmailService emailService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        // GET: ChiTieu
        public async Task<IActionResult> Index(string danhMucFilter, string searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            // Thêm logic để hiển thị thông báo phản hồi
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = _context.ChiTieus.Where(c => c.NguoiDungId == userId).Include(c => c.DanhMuc).OrderByDescending(c => c.NgayChi).AsQueryable();

            if (!string.IsNullOrEmpty(searchString)) query = query.Where(s => s.TenChiTieu.Contains(searchString) || (s.GhiChu != null && s.GhiChu.Contains(searchString)));
            if (!string.IsNullOrEmpty(danhMucFilter)) query = query.Where(x => x.DanhMuc.TenDanhMuc == danhMucFilter);
            if (tuNgay.HasValue) query = query.Where(x => x.NgayChi.HasValue && x.NgayChi.Value.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(x => x.NgayChi.HasValue && x.NgayChi.Value.Date <= denNgay.Value.Date);

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
            var chiTieu = await _context.ChiTieus.Include(c => c.DanhMuc).FirstOrDefaultAsync(m => m.Id == id && m.NguoiDungId == userId);
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
                TempData["SuccessMessage"] = "Thêm khoản chi mới thành công!";

                // Gọi hàm kiểm tra hạn mức và gửi thông báo
                await CheckLimitAndNotify(userId, chiTieu.NgayChi.Value);

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
                var originalChiTieu = await _context.ChiTieus.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.NguoiDungId == userId);
                if (originalChiTieu == null) return NotFound();
                chiTieu.NguoiDungId = userId;

                try
                {
                    _context.Update(chiTieu);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật khoản chi thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTieuExists(chiTieu.Id)) return NotFound(); else throw;
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
            var chiTieu = await _context.ChiTieus.Include(c => c.DanhMuc).FirstOrDefaultAsync(m => m.Id == id && m.NguoiDungId == userId);
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
                TempData["SuccessMessage"] = "Đã xóa khoản chi thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuggestCategory(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Json(new { success = false });
            var keyword = await _context.TuKhoaDanhMucs.FirstOrDefaultAsync(k => query.ToLower().Contains(k.TuKhoa.ToLower()));
            if (keyword != null) return Json(new { success = true, categoryId = keyword.DanhMucId });
            return Json(new { success = false });
        }

        private bool ChiTieuExists(int id) => _context.ChiTieus.Any(e => e.Id == id);

        // HÀM HỖ TRỢ KIỂM TRA HẠN MỨC VÀ GỬI THÔNG BÁO
        private async Task CheckLimitAndNotify(int userId, DateTime ngayChi)
        {
            var hanMuc = await _context.GioiHanChiTieus.FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.Thang == ngayChi.Month && g.Nam == ngayChi.Year);
            if (hanMuc == null || hanMuc.SoTienToiDa <= 0) return;

            var tongChiTieu = await _context.ChiTieus
                .Where(c => c.NguoiDungId == userId && c.NgayChi.HasValue && c.NgayChi.Value.Month == ngayChi.Month && c.NgayChi.Value.Year == ngayChi.Year)
                .SumAsync(c => c.SoTien);

            if (tongChiTieu > hanMuc.SoTienToiDa)
            {
                var user = await _context.NguoiDungs.FindAsync(userId);
                if (user != null)
                {
                    var vuotMuc = tongChiTieu - hanMuc.SoTienToiDa;
                    var message = $"Bạn đã vượt {vuotMuc:N0} ₫ so với hạn mức tháng này!";

                    // Gửi thông báo real-time qua SignalR
                    await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", message, "warning");

                    // Gửi email
                    var subject = "⚠️ Cảnh báo: Vượt hạn mức chi tiêu";
                    var emailMessage = $"<p>Chào {user.HoTen ?? "bạn"},</p><p>Hệ thống ghi nhận bạn đã chi tiêu vượt hạn mức trong tháng {ngayChi.Month}/{ngayChi.Year}.</p><ul><li><strong>Hạn mức:</strong> {hanMuc.SoTienToiDa:N0} ₫</li><li><strong>Tổng chi:</strong> {tongChiTieu:N0} ₫</li><li><strong>Số tiền vượt:</strong> {vuotMuc:N0} ₫</li></ul><p>Vui lòng xem lại các khoản chi của mình.</p>";
                    await _emailService.SendEmailAsync(user.Email, subject, emailMessage);
                }
            }
        }
    }
}