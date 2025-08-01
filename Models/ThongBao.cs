using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("ThongBao")]
[Index("NguoiDungId", Name = "IX_ThongBao_NguoiDungId")]
public partial class ThongBao
{
    [Key]
    public int Id { get; set; }

    public string NoiDung { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? NgayGui { get; set; }

    public int NguoiDungId { get; set; }

    public bool? DaDoc { get; set; }

    [ForeignKey("NguoiDungId")]
    [InverseProperty("ThongBaos")]
    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
