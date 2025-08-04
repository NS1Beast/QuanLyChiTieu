namespace QuanLyChiTieu.ViewModels
{
    public class QuickAddResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AlertStatus { get; set; } = "success"; // Dùng cho class CSS: success, info, warning, danger
    }
}
