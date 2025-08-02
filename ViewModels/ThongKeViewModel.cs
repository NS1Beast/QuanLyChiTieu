using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyChiTieu.ViewModels
{
    // Lớp này chứa thông tin chi tiêu của một danh mục
    public class CategorySpending
    {
        public string TenDanhMuc { get; set; }
        public string MauSac { get; set; }
        public decimal TongTien { get; set; }
        public double TyTrong { get; set; } // Tỷ trọng %
    }

    // ViewModel chính cho trang Thống kê
    public class ThongKeViewModel
    {
        public int SelectedYear { get; set; }
        public SelectList YearList { get; set; }
        public string JsonMonthlySpending { get; set; } // Dữ liệu cho biểu đồ đường
        public List<CategorySpending> CategoryBreakdown { get; set; } // Dữ liệu cho bảng phân tích
        public decimal TotalYearlySpending { get; set; } // Tổng chi tiêu của năm
    }
}