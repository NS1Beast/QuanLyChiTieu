using QuanLyChiTieu.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyChiTieu.ViewModels
{
    public class ChiTieuIndexViewModel
    {
        // Danh sách chi tiêu để hiển thị
        public List<ChiTieu> ChiTieuList { get; set; } = new List<ChiTieu>();

        // Dữ liệu cho các bộ lọc
        public SelectList? DanhMucList { get; set; }
        public string? DanhMucFilter { get; set; }
        public string? SearchString { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        // Dữ liệu cho phần tổng kết
        public decimal TongChiTieu { get; set; }
    }
}