using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyChiTieu.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TuDongPhanLoai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMuc__3214EC07C2C35FB3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayDangKy = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    LoaiTaiKhoan = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    NhanEmailNhacNho = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    TanSuatNhanNhac = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "HangTuan"),
                    GioNhanNhac = table.Column<TimeOnly>(type: "time", nullable: true, defaultValue: new TimeOnly(18, 0, 0))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NguoiDun__3214EC07CF5C7E6E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OTP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaOTP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OTP__3214EC0766DF5851", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TuKhoaDanhMuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanhMucId = table.Column<int>(type: "int", nullable: false),
                    TuKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TuKhoaDa__3214EC077923D733", x => x.Id);
                    table.ForeignKey(
                        name: "FK__TuKhoaDan__DanhM__52593CB8",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMuc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChiTieu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayChi = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    DanhMucId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiTieu__3214EC0748AA8F80", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ChiTieu__DanhMuc__440B1D61",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMuc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ChiTieu__NguoiDu__4316F928",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioiHanChiTieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoTienToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GioiHanC__3214EC078D4B0F17", x => x.Id);
                    table.ForeignKey(
                        name: "FK__GioiHanCh__Nguoi__46E78A0C",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuNhanNhac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    LoaiNhanNhac = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LichSuNh__3214EC07A3A7D1EB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__LichSuNha__Nguoi__571DF1D5",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ThongBao__3214EC07F56E0324", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ThongBao__NguoiD__4BAC3F29",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieu_DanhMucId",
                table: "ChiTieu",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieu_NgayChi",
                table: "ChiTieu",
                column: "NgayChi");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieu_NguoiDungId",
                table: "ChiTieu",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_GioiHanChiTieu_NguoiDungId",
                table: "GioiHanChiTieu",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_GioiHanChiTieu_Thang_Nam",
                table: "GioiHanChiTieu",
                columns: new[] { "Thang", "Nam" });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhanNhac_NgayGui",
                table: "LichSuNhanNhac",
                column: "NgayGui");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhanNhac_NguoiDungId",
                table: "LichSuNhanNhac",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "UQ__NguoiDun__A9D10534A0F8E828",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiDungId",
                table: "ThongBao",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_TuKhoaDanhMuc_DanhMucId",
                table: "TuKhoaDanhMuc",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_TuKhoaDanhMuc_TuKhoa",
                table: "TuKhoaDanhMuc",
                column: "TuKhoa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTieu");

            migrationBuilder.DropTable(
                name: "GioiHanChiTieu");

            migrationBuilder.DropTable(
                name: "LichSuNhanNhac");

            migrationBuilder.DropTable(
                name: "OTP");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "TuKhoaDanhMuc");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "DanhMuc");
        }
    }
}
