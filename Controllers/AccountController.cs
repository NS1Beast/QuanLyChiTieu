using Microsoft.AspNetCore.Mvc;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using QuanLyChiTieu.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // <-- THÊM using này
using Microsoft.AspNetCore.Authentication; // <-- THÊM using này

namespace QuanLyChiTieu.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataBase_DoAnContext _context;
        private readonly IEmailService _emailService;

        // Tiêm DbContext và IEmailService vào constructor
        public AccountController(DataBase_DoAnContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // --- HÀM HASH MẬT KHẨU (VÍ DỤ ĐƠN GIẢN) ---
        // Trong thực tế nên dùng thư viện như BCrypt.Net để an toàn hơn
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // --- HÀM HỖ TRỢ ĐĂNG NHẬP ---
        // Tách riêng hàm này để có thể tái sử dụng cho cả Login và sau khi Register
        private async Task SignInUserAsync(NguoiDung user)
        {
            // Tạo các "claims" - thông tin về người dùng đã đăng nhập
            var claims = new List<Claim>
            {
                // Claim chứa Email, có thể dùng để hiển thị lời chào
                new Claim(ClaimTypes.Name, user.Email),
                // Claim chứa ID của người dùng, rất quan trọng để truy vấn dữ liệu của riêng họ
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Thêm các claim khác nếu cần, ví dụ như Họ Tên
                new Claim("FullName", user.HoTen ?? "")
            };

            // Tạo danh tính người dùng
            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            // Tạo cookie xác thực
            var authProperties = new AuthenticationProperties
            {
                // IsPersistent = true sẽ giúp cookie tồn tại sau khi đóng trình duyệt
                IsPersistent = true
            };

            // Thực hiện đăng nhập, hệ thống sẽ tạo và gửi cookie về trình duyệt
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
        }


        // --- ĐĂNG KÝ ---
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
                // 1. Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
                    return View(model);
                }

                // 2. Tạo mã OTP
                var otpCode = new Random().Next(100000, 999999).ToString();
                var otp = new Otp
                {
                    Email = model.Email,
                    MaOtp = otpCode,
                    ThoiGianTao = DateTime.Now,
                    TrangThai = false // False = chưa được xác thực
                };

                // 3. Lưu OTP vào database
                _context.Otps.Add(otp);
                await _context.SaveChangesAsync();

                // 4. Lưu thông tin đăng ký tạm thời vào TempData
                // TempData chỉ tồn tại trong một yêu cầu chuyển hướng.
                TempData["PendingUserEmail"] = model.Email;
                TempData["PendingUserPassword"] = HashPassword(model.Password);

                // 5. Gửi email chứa mã OTP
                var subject = "Mã OTP xác thực tài khoản";
                var message = $"<p>Chào bạn,</p><p>Mã OTP để xác thực tài khoản của bạn là: <strong>{otpCode}</strong></p><p>Mã này sẽ hết hạn sau 5 phút.</p>";
                await _emailService.SendEmailAsync(model.Email, subject, message);

                // 6. Chuyển hướng đến trang nhập OTP
                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }
            return View(model);
        }

        // --- XÁC THỰC OTP ---
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }
            var model = new VerifyOtpViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            // Giữ lại dữ liệu TempData để nếu người dùng nhập sai OTP, họ có thể thử lại.
            TempData.Keep("PendingUserEmail");
            TempData.Keep("PendingUserPassword");

            if (ModelState.IsValid)
            {
                // Tìm mã OTP hợp lệ trong database (chưa dùng, chưa hết hạn)
                var otpRecord = await _context.Otps
                  .FirstOrDefaultAsync(o => o.Email == model.Email
                       && o.MaOtp == model.Otp
                       && o.TrangThai == false
                       && o.ThoiGianTao.HasValue // 1. Kiểm tra xem ThoiGianTao có giá trị không
                       && o.ThoiGianTao.Value.AddMinutes(5) > DateTime.Now); // 2. Lấy giá trị rồi mới cộng thêm phút

                if (otpRecord == null)
                {
                    ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                // Lấy thông tin người dùng đang chờ từ TempData
                var email = TempData["PendingUserEmail"] as string;
                var hashedPassword = TempData["PendingUserPassword"] as string;

                if (email != model.Email || string.IsNullOrEmpty(hashedPassword))
                {
                    ModelState.AddModelError(string.Empty, "Phiên đăng ký đã hết hạn, vui lòng thử lại.");
                    return View(model);
                }

                // Tạo người dùng mới
                var newUser = new NguoiDung
                {
                    Email = email,
                    MatKhau = hashedPassword,
                    // Các trường khác sẽ có giá trị mặc định từ database
                };

                _context.NguoiDungs.Add(newUser);

                // Đánh dấu OTP đã được sử dụng
                otpRecord.TrangThai = true;

                await _context.SaveChangesAsync();

                // *** TỰ ĐỘNG ĐĂNG NHẬP SAU KHI ĐĂNG KÝ THÀNH CÔNG ***
                await SignInUserAsync(newUser);

                // Chuyển đến trang chính
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng bằng email
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);

                // Nếu tìm thấy người dùng, kiểm tra mật khẩu
                if (user != null)
                {
                    // Hash mật khẩu người dùng nhập vào và so sánh với hash trong database
                    if (user.MatKhau == HashPassword(model.Password))
                    {
                        // Nếu mật khẩu khớp, thực hiện đăng nhập
                        await SignInUserAsync(user);
                        return RedirectToAction("Index", "Dashboard"); // Chuyển đến trang dashboard
                    }
                }

                // Nếu không tìm thấy user hoặc mật khẩu không đúng
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            }
            return View(model);
        }


        // --- ĐĂNG XUẤT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync("MyCookieAuth");
            // Chuyển về trang giới thiệu
            return RedirectToAction("Index", "Home");
        }

        // --- TRANG TRUY CẬP BỊ TỪ CHỐI ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}