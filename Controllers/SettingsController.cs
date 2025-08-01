using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly DataBase_DoAnContext _context;

        public SettingsController(DataBase_DoAnContext context)
        {
            _context = context;
        }

        // GET: /Settings
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);

            if (user == null) return NotFound();

            var viewModel = new SettingsViewModel
            {
                NhanEmailNhacNho = user.NhanEmailNhacNho ?? true,
                TanSuatNhanNhac = user.TanSuatNhanNhac,
                GioNhanNhac = user.GioNhanNhac ?? new TimeOnly(18, 0)
            };

            return View(viewModel);
        }

        // POST: /Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.NguoiDungs.FindAsync(userId);

                if (user == null) return NotFound();

                user.NhanEmailNhacNho = model.NhanEmailNhacNho;
                user.TanSuatNhanNhac = model.TanSuatNhanNhac;
                user.GioNhanNhac = model.GioNhanNhac;

                _context.Update(user);
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Cập nhật cài đặt thành công!";
            }
            return View(model);
        }
    }
}