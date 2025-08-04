using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.ViewModels
{
    public class ProfileViewModel
    {
        public string? Email { get; set; }

        // CẬP NHẬT: Thêm các ràng buộc
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và Tên")]
        public string? HoTen { get; set; }

        public string? AvatarUrl { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }
    }
}