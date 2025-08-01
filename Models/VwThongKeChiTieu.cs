using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Keyless]
public partial class VwThongKeChiTieu
{
    public int NguoiDungId { get; set; }

    [StringLength(100)]
    public string TenDanhMuc { get; set; } = null!;

    public int? Thang { get; set; }

    public int? Nam { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal? TongTien { get; set; }
}
