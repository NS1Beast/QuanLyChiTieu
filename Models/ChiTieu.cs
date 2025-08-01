using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("ChiTieu")]
[Index("NgayChi", Name = "IX_ChiTieu_NgayChi")]
[Index("NguoiDungId", Name = "IX_ChiTieu_NguoiDungId")]
public partial class ChiTieu
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string TenChiTieu { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTien { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayChi { get; set; }

    public string? GhiChu { get; set; }

    public int NguoiDungId { get; set; }

    public int DanhMucId { get; set; }

    [ForeignKey("DanhMucId")]
    [InverseProperty("ChiTieus")]
    public virtual DanhMuc DanhMuc { get; set; } = null!;

    [ForeignKey("NguoiDungId")]
    [InverseProperty("ChiTieus")]
    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
