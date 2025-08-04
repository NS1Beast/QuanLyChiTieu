using System.Collections.Generic;

namespace QuanLyChiTieu.ViewModels
{
    // Lớp này đại diện cho một người trong top chi tiêu (ẩn danh)
    public class TopSpenderViewModel
    {
        public int Rank { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // ViewModel chính cho trang xếp hạng
    public class XepHangViewModel
    {
        public List<TopSpenderViewModel> TopSpenders { get; set; } = new List<TopSpenderViewModel>();
        public int CurrentUserRank { get; set; }
        public decimal CurrentUserSpending { get; set; }
        public int TotalUsersRanked { get; set; }
    }
}