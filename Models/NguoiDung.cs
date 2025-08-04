using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("NguoiDung")]
[Index("Email", Name = "UQ__NguoiDun__A9D10534A0F8E828", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string MatKhau { get; set; } = null!;

    [StringLength(100)]
    public string? HoTen { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayDangKy { get; set; }

    public int? LoaiTaiKhoan { get; set; }

    public bool? NhanEmailNhacNho { get; set; }

    [StringLength(10)]
    public string? TanSuatNhanNhac { get; set; }

    public TimeOnly? GioNhanNhac { get; set; }

    [StringLength(512)]
    public string? AvatarUrl { get; set; }

    [InverseProperty("NguoiDung")]
    public virtual ICollection<ChiTieu> ChiTieus { get; set; } = new List<ChiTieu>();

    [InverseProperty("NguoiDung")]
    public virtual ICollection<GioiHanChiTieu> GioiHanChiTieus { get; set; } = new List<GioiHanChiTieu>();

    [InverseProperty("NguoiDung")]
    public virtual ICollection<LichSuNhanNhac> LichSuNhanNhacs { get; set; } = new List<LichSuNhanNhac>();

    [InverseProperty("NguoiDung")]
    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
