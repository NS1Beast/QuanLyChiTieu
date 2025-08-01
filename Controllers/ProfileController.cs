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
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.SuccessMessage = message;
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);

            if (user == null) return NotFound();

            var viewModel = new ProfileViewModel
            {
                Email = user.Email,
                HoTen = user.HoTen
            };
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

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // CẬP NHẬT: Thêm kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Vui lòng kiểm tra lại các thông tin đã nhập.";
                return RedirectToAction("Index");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);

            if (user == null) return NotFound();

            if (user.MatKhau != HashPassword(model.OldPassword))
            {
                TempData["PasswordError"] = "Mật khẩu cũ không chính xác.";
                return RedirectToAction("Index");
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new Otp
            {
                Email = user.Email,
                MaOtp = otpCode,
                ThoiGianTao = DateTime.Now
            };
            _context.Otps.Add(otp);
            await _context.SaveChangesAsync();

            TempData["NewHashedPassword"] = HashPassword(model.NewPassword);

            var subject = "Mã OTP xác nhận đổi mật khẩu";
            var message = $"Mã OTP để xác nhận đổi mật khẩu của bạn là: <strong>{otpCode}</strong>";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            return RedirectToAction("VerifyPasswordChange");
        }

        // GET: /Profile/VerifyPasswordChange
        public IActionResult VerifyPasswordChange()
        {
            // Giữ lại TempData để nếu người dùng reload trang không bị mất
            TempData.Keep("NewHashedPassword");
            return View();
        }

        // POST: /Profile/VerifyPasswordChange
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPasswordChange(string otp)
        {
            var email = User.Identity.Name;
            var newHashedPassword = TempData["NewHashedPassword"] as string;

            if (string.IsNullOrEmpty(newHashedPassword))
            {
                ModelState.AddModelError(string.Empty, "Phiên làm việc đã hết hạn. Vui lòng thử lại.");
                return View();
            }

            var otpRecord = await _context.Otps
                .FirstOrDefaultAsync(o => o.Email == email && o.MaOtp == otp && o.TrangThai == false && o.ThoiGianTao.HasValue && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now);

            if (otpRecord == null)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
                TempData.Keep("NewHashedPassword"); // Giữ lại pass để user thử lại
                return View();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);

            if (user == null) return NotFound();

            user.MatKhau = newHashedPassword;
            otpRecord.TrangThai = true;

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { message = "Đổi mật khẩu thành công!" });
        }
    }
}