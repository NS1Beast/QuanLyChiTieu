using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.ViewModels
{
    // Dùng để nhận dữ liệu từ form xác thực OTP
    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        public string Otp { get; set; }
    }
}