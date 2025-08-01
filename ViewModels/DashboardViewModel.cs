using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.ViewModels
{
    public class DashboardViewModel
    {
        // Dữ liệu cho các thẻ thống kê
        public decimal TongChiThangNay { get; set; }
        public decimal ChiTieuHomNay { get; set; }
        public int SoGiaoDichThangNay { get; set; }
        public string? DanhMucChiNhieuNhat { get; set; }

        // === THÊM THUỘC TÍNH NÀY ===
        public decimal HanMucThangNay { get; set; }

        // Dữ liệu đã được xử lý thành JSON cho biểu đồ
        public string JsonChiTieu7NgayGanNhat { get; set; } = "{}";
        public string JsonChiTieuTheoDanhMuc { get; set; } = "{}";

        // Dữ liệu cho bảng giao dịch gần đây
        public List<ChiTieu> GiaoDichGanDay { get; set; } = new List<ChiTieu>();

        // === THÊM THUỘC TÍNH NÀY ===
        // Dictionary với Key là tên danh mục (string), Value là số tiền đề xuất (decimal)
        public Dictionary<string, decimal>? DeXuatAI { get; set; }
        public List<DanhMuc> TatCaDanhMuc { get; set; } = new List<DanhMuc>();
    }
}