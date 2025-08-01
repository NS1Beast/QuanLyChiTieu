using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("GioiHanChiTieu")]
[Index("Thang", "Nam", Name = "IX_GioiHanChiTieu_Thang_Nam")]
public partial class GioiHanChiTieu
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTienToiDa { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    public int NguoiDungId { get; set; }

    [ForeignKey("NguoiDungId")]
    [InverseProperty("GioiHanChiTieus")]
    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
