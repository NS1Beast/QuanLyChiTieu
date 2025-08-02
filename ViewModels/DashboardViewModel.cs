using QuanLyChiTieu.Models;
using System.Collections.Generic;

namespace QuanLyChiTieu.ViewModels
{
    public class DashboardViewModel
    {
        // Dữ liệu cho các thẻ thống kê
        public decimal TongChiThangNay { get; set; }
        public decimal ChiTieuHomNay { get; set; }
        public int SoGiaoDichThangNay { get; set; }
        public string? DanhMucChiNhieuNhat { get; set; }
        public decimal HanMucThangNay { get; set; }

        // Dữ liệu JSON cho biểu đồ
        public string JsonChiTieu7NgayGanNhat { get; set; } = "{}";
        public string JsonChiTieuTheoDanhMuc { get; set; } = "{}";

        // Dữ liệu cho bảng và modal "Thêm nhanh"
        public List<ChiTieu> GiaoDichGanDay { get; set; } = new List<ChiTieu>();
        public List<DanhMuc> TatCaDanhMuc { get; set; } = new List<DanhMuc>();
    }
}