using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("TuKhoaDanhMuc")]
[Index("TuKhoa", Name = "IX_TuKhoaDanhMuc_TuKhoa")]
public partial class TuKhoaDanhMuc
{
    [Key]
    public int Id { get; set; }

    public int DanhMucId { get; set; }

    [StringLength(100)]
    public string TuKhoa { get; set; } = null!;

    [ForeignKey("DanhMucId")]
    [InverseProperty("TuKhoaDanhMucs")]
    public virtual DanhMuc DanhMuc { get; set; } = null!;
}
