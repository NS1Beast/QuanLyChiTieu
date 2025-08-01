using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.ViewModels
{
    public class GioiHanChiTieuViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền.")]
        [Range(1, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        [Display(Name = "Số tiền tối đa")]
        public decimal SoTienToiDa { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Tháng không hợp lệ.")]
        public int Thang { get; set; }

        [Required]
        [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ.")]
        public int Nam { get; set; }
    }
}