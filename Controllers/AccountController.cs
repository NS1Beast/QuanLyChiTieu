using Microsoft.AspNetCore.Mvc;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using QuanLyChiTieu.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace QuanLyChiTieu.Controllers
{
    [AllowAnonymous] // Cho phép tất cả người dùng truy cập các action trong controller này
    public class AccountController : Controller
    {
        private readonly DataBase_DoAnContext _context;
        private readonly IEmailService _emailService;

        public AccountController(DataBase_DoAnContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Băm mật khẩu bằng thuật toán SHA256.
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Tạo cookie xác thực cho người dùng.
        /// </summary>
        /// <param name="user">Đối tượng người dùng.</param>
        /// <param name="isPersistent">True nếu người dùng chọn "Ghi nhớ tôi".</param>
        private async Task SignInUserAsync(NguoiDung user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                // Claim Name dùng để hiển thị (VD: Chào, Anh Tuấn)
                new Claim(ClaimTypes.Name, !string.IsNullOrWhiteSpace(user.HoTen) ? user.HoTen : user.Email),
                // Claim Email để sử dụng nội bộ khi cần
                new Claim(ClaimTypes.Email, user.Email),
                // Claim NameIdentifier chứa ID của người dùng, rất quan trọng
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        #region Đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
                    return View(model);
                }

                var otpCode = new Random().Next(100000, 999999).ToString();
                var otp = new Otp { Email = model.Email, MaOtp = otpCode, ThoiGianTao = DateTime.Now };
                _context.Otps.Add(otp);
                await _context.SaveChangesAsync();

                TempData["PendingUserEmail"] = model.Email;
                TempData["PendingUserPassword"] = HashPassword(model.Password);

                var subject = "Mã OTP xác thực tài khoản";
                var message = $"<p>Chào bạn,</p><p>Mã OTP để xác thực tài khoản của bạn là: <strong>{otpCode}</strong></p><p>Mã này sẽ hết hạn sau 5 phút.</p>";
                await _emailService.SendEmailAsync(model.Email, subject, message);

                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");
            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            TempData.Keep("PendingUserEmail");
            TempData.Keep("PendingUserPassword");

            if (ModelState.IsValid)
            {
                var otpRecord = await _context.Otps.FirstOrDefaultAsync(o => o.Email == model.Email && o.MaOtp == model.Otp && o.TrangThai == false && o.ThoiGianTao.HasValue && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now);
                if (otpRecord == null)
                {
                    ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                var email = TempData["PendingUserEmail"] as string;
                var hashedPassword = TempData["PendingUserPassword"] as string;
                if (email != model.Email || string.IsNullOrEmpty(hashedPassword))
                {
                    ModelState.AddModelError(string.Empty, "Phiên đăng ký đã hết hạn, vui lòng thử lại.");
                    return View(model);
                }

                var newUser = new NguoiDung { Email = email, MatKhau = hashedPassword, NgayDangKy = DateTime.Now };
                _context.NguoiDungs.Add(newUser);
                otpRecord.TrangThai = true;
                await _context.SaveChangesAsync();

                await SignInUserAsync(newUser, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }
        #endregion

        #region Đăng nhập / Đăng xuất
        [HttpGet]
        public IActionResult Login()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password);
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email && u.MatKhau == hashedPassword);
                if (user != null)
                {
                    await SignInUserAsync(user, model.RememberMe);
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Quên & Đặt lại mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng nhập email." });
            }
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Json(new { success = true, message = "Nếu email tồn tại, mã OTP đã được gửi." });
            }
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new Otp { Email = user.Email, MaOtp = otpCode, ThoiGianTao = DateTime.Now };
            _context.Otps.Add(otp);
            await _context.SaveChangesAsync();
            var subject = "Yêu cầu đặt lại mật khẩu";
            var message = $"Mã OTP để đặt lại mật khẩu của bạn là: <strong>{otpCode}</strong>";
            await _emailService.SendEmailAsync(user.Email, subject, message);
            return Json(new { success = true, message = "Mã OTP đã được gửi thành công." });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
                var otpRecord = await _context.Otps.FirstOrDefaultAsync(o => o.Email == model.Email && o.MaOtp == model.Otp && o.TrangThai == false && o.ThoiGianTao.HasValue && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now);

                if (user == null || otpRecord == null)
                {
                    ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                user.MatKhau = HashPassword(model.NewPassword);
                otpRecord.TrangThai = true;
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bây giờ bạn có thể đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }
        #endregion
    }
}