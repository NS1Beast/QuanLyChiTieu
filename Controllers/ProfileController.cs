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
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly DataBase_DoAnContext _context;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(DataBase_DoAnContext context, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
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

            var viewModel = new ProfileViewModel
            {
                Email = user.Email,
                HoTen = user.HoTen,
                AvatarUrl = user.AvatarUrl
            };
            return View(viewModel);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.HoTen = model.HoTen;

                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(model.AvatarFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AvatarFile.CopyToAsync(fileStream);
                    }
                    user.AvatarUrl = "/uploads/avatars/" + uniqueFileName;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { message = "Cập nhật thông tin thành công!" });
            }
            model.AvatarUrl = user.AvatarUrl;
            return View("Index", model);
        }

        // POST: /Profile/SendPasswordChangeOtp
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

        // POST: /Profile/ChangePassword
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

            if (user.MatKhau != HashPassword(model.OldPassword))
            {
                TempData["PasswordError"] = "Mật khẩu cũ không chính xác.";
                return RedirectToAction("Index");
            }

            var otpRecord = await _context.Otps
                .FirstOrDefaultAsync(o => o.Email == user.Email && o.MaOtp == model.Otp && o.TrangThai == false && o.ThoiGianTao.HasValue && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now);

            if (otpRecord == null)
            {
                TempData["PasswordError"] = "Mã OTP không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Index");
            }

            user.MatKhau = HashPassword(model.NewPassword);
            otpRecord.TrangThai = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { message = "Đổi mật khẩu thành công!" });
        }

        // POST: /Profile/SendDeleteAccountOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendDeleteAccountOtp()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) { return Json(new { success = false, message = "Không tìm thấy người dùng." }); }
            
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new Otp { Email = user.Email, MaOtp = otpCode, ThoiGianTao = DateTime.Now };
            _context.Otps.Add(otp);
            await _context.SaveChangesAsync();
            
            var subject = "Yêu cầu Xóa Tài khoản - Mã OTP Xác nhận";
            var message = $"<p>Chào {user.HoTen ?? "bạn"},</p><p>Chúng tôi đã nhận được yêu cầu xóa tài khoản của bạn. Vui lòng sử dụng mã OTP sau để xác nhận. <strong>Lưu ý: Hành động này không thể hoàn tác.</strong></p><p>Mã OTP của bạn là: <strong>{otpCode}</strong></p>";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn." });
        }

        // POST: /Profile/DeleteAccountConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed(string otp)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) { return RedirectToAction("Index", "Home"); }

            var otpRecord = await _context.Otps.FirstOrDefaultAsync(o => o.Email == user.Email && o.MaOtp == otp && o.TrangThai == false && o.ThoiGianTao.HasValue && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now);
            if (otpRecord == null)
            {
                TempData["PasswordError"] = "Mã OTP không hợp lệ hoặc đã hết hạn. Yêu cầu xóa tài khoản đã bị hủy.";
                return RedirectToAction("Index");
            }
            
            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync("MyCookieAuth");
            
            TempData["SuccessMessage"] = "Tài khoản của bạn đã được xóa thành công. Cảm ơn bạn đã sử dụng dịch vụ!";
            return RedirectToAction("Index", "Home");
        }
    }
}