using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Data;

public partial class DataBase_DoAnContext : DbContext
{
    public DataBase_DoAnContext()
    {
    }

    public DataBase_DoAnContext(DbContextOptions<DataBase_DoAnContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTieu> ChiTieus { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<GioiHanChiTieu> GioiHanChiTieus { get; set; }

    public virtual DbSet<LichSuNhanNhac> LichSuNhanNhacs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TuKhoaDanhMuc> TuKhoaDanhMucs { get; set; }

    public virtual DbSet<VwThongKeChiTieu> VwThongKeChiTieus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTieu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTieu__3214EC0748AA8F80");

            entity.ToTable("ChiTieu", tb => tb.HasTrigger("trg_CanhBaoVuotHanMuc"));

            entity.Property(e => e.NgayChi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.ChiTieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTieu__DanhMuc__440B1D61");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.ChiTieus).HasConstraintName("FK__ChiTieu__NguoiDu__4316F928");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMuc__3214EC07C2C35FB3");

            entity.Property(e => e.TuDongPhanLoai).HasDefaultValue(true);
        });

        modelBuilder.Entity<GioiHanChiTieu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GioiHanC__3214EC078D4B0F17");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.GioiHanChiTieus).HasConstraintName("FK__GioiHanCh__Nguoi__46E78A0C");
        });

        modelBuilder.Entity<LichSuNhanNhac>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LichSuNh__3214EC07A3A7D1EB");

            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.LichSuNhanNhacs).HasConstraintName("FK__LichSuNha__Nguoi__571DF1D5");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NguoiDun__3214EC07CF5C7E6E");

            entity.Property(e => e.GioNhanNhac).HasDefaultValue(new TimeOnly(18, 0, 0));
            entity.Property(e => e.LoaiTaiKhoan).HasDefaultValue(0);
            entity.Property(e => e.NgayDangKy).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NhanEmailNhacNho).HasDefaultValue(true);
            entity.Property(e => e.TanSuatNhanNhac).HasDefaultValue("HangTuan");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTP__3214EC0766DF5851");

            entity.Property(e => e.ThoiGianTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue(false);
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongBao__3214EC07F56E0324");

            entity.Property(e => e.DaDoc).HasDefaultValue(false);
            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.ThongBaos).HasConstraintName("FK__ThongBao__NguoiD__4BAC3F29");
        });

        modelBuilder.Entity<TuKhoaDanhMuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TuKhoaDa__3214EC077923D733");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.TuKhoaDanhMucs).HasConstraintName("FK__TuKhoaDan__DanhM__52593CB8");
        });

        modelBuilder.Entity<VwThongKeChiTieu>(entity =>
        {
            entity.ToView("vw_ThongKeChiTieu");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
