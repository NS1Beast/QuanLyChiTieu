using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.ViewModels
{
    // ViewModel này đại diện cho một dòng trong bảng hạn mức
    public class GioiHanChiTieuItemViewModel
    {
        public GioiHanChiTieu GioiHan { get; set; }
        public decimal TongChiTieuThang { get; set; }
        public int PhanTramDaDung { get; set; }
    }
}