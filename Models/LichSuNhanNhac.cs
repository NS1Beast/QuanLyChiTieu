using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("LichSuNhanNhac")]
[Index("NgayGui", Name = "IX_LichSuNhanNhac_NgayGui")]
public partial class LichSuNhanNhac
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? NgayGui { get; set; }

    public bool? TrangThai { get; set; }

    [StringLength(50)]
    public string LoaiNhanNhac { get; set; } = null!;

    public int NguoiDungId { get; set; }

    [ForeignKey("NguoiDungId")]
    [InverseProperty("LichSuNhanNhacs")]
    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
