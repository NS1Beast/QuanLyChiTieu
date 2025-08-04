using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("DanhMuc")]
public partial class DanhMuc
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string TenDanhMuc { get; set; } = null!;

    public bool? TuDongPhanLoai { get; set; }

    [StringLength(20)]
    public string? MauSac { get; set; }

    [InverseProperty("DanhMuc")]
    public virtual ICollection<ChiTieuTheoLich> ChiTieuTheoLiches { get; set; } = new List<ChiTieuTheoLich>();

    [InverseProperty("DanhMuc")]
    public virtual ICollection<ChiTieu> ChiTieus { get; set; } = new List<ChiTieu>();

    [InverseProperty("DanhMuc")]
    public virtual ICollection<TuKhoaDanhMuc> TuKhoaDanhMucs { get; set; } = new List<TuKhoaDanhMuc>();
}
