using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly DataBase_DoAnContext _context;
        private readonly IEmailService _emailService;

        public ProfileController(DataBase_DoAnContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // GET: /Profile
        public async Task<IActionResult> Index(string? message)
        {
            if (!string.IsNullOrEmpty(message)) ViewBag.SuccessMessage = message;
            if (TempData["PasswordError"] != null) ViewBag.ErrorMessage = TempData["PasswordError"];

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return NotFound();

            var viewModel = new ProfileViewModel { Email = user.Email, HoTen = user.HoTen };
            return View(viewModel);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.NguoiDungs.FindAsync(userId);
                if (user == null) return NotFound();

                user.HoTen = model.HoTen;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { message = "Cập nhật thông tin thành công!" });
            }
            return View("Index", model);
        }

        // ACTION MỚI: Chỉ gửi mã OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPasswordChangeOtp()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new Otp { Email = user.Email, MaOtp = otpCode, ThoiGianTao = DateTime.Now };
            _context.Otps.Add(otp);
            await _context.SaveChangesAsync();

            var subject = "Mã OTP xác nhận đổi mật khẩu";
            var message = $"Mã OTP để xác nhận đổi mật khẩu của bạn là: <strong>{otpCode}</strong>";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn." });
        }

        // ACTION ĐƯỢC CẬP NHẬT: Xử lý toàn bộ form đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["PasswordError"] = "Vui lòng kiểm tra lại thông tin: " + string.Join("; ", errors);
                return RedirectToAction("Index");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return NotFound();

            // 1. Kiểm tra mật khẩu cũ
            if (user.MatKhau != HashPassword(model.OldPassword))
            {
                TempData["PasswordError"] = "Mật khẩu cũ không chính xác.";
                return RedirectToAction("Index");
            }

            // 2. Kiểm tra OTP
            var otpRecord = await _context.Otps
                .FirstOrDefaultAsync(o => o.Email == user.Email && o.MaOtp == model.Otp && o.TrangThai == false && o.ThoiGianTao.HasValue && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now);

            if (otpRecord == null)
            {
                TempData["PasswordError"] = "Mã OTP không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Index");
            }

            // 3. Mọi thứ hợp lệ, tiến hành đổi mật khẩu
            user.MatKhau = HashPassword(model.NewPassword);
            otpRecord.TrangThai = true; // Đánh dấu OTP đã sử dụng

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { message = "Đổi mật khẩu thành công!" });
        }
    }
}