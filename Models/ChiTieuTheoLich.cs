using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("ChiTieuTheoLich")]
public partial class ChiTieuTheoLich
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string TenChiTieu { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTien { get; set; }

    public string? GhiChu { get; set; }

    public DateOnly? LanThucHienCuoi { get; set; }

    public bool HoatDong { get; set; }

    public int NguoiDungId { get; set; }

    public int DanhMucId { get; set; }

    public DateOnly NgayThucHien { get; set; }

    [ForeignKey("DanhMucId")]
    [InverseProperty("ChiTieuTheoLiches")]
    public virtual DanhMuc DanhMuc { get; set; } = null!;

    [ForeignKey("NguoiDungId")]
    [InverseProperty("ChiTieuTheoLiches")]
    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
