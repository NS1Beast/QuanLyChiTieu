using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.ViewModels
{
    public class SettingsViewModel
    {
        public bool NhanEmailNhacNho { get; set; }

        public string? TanSuatNhanNhac { get; set; } // Ví dụ: 'HangNgay', 'HangTuan'

        [DataType(DataType.Time)]
        public TimeOnly GioNhanNhac { get; set; }
    }
}