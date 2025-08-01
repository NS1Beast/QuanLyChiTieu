using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.ViewModels
{
    public class ProfileViewModel
    {
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string? HoTen { get; set; }
    }
}