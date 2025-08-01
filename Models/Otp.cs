using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

[Table("OTP")]
public partial class Otp
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("MaOTP")]
    [StringLength(10)]
    public string MaOtp { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianTao { get; set; }

    public bool? TrangThai { get; set; }
}
