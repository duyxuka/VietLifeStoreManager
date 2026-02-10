using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class TaoEntityDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomer",
                table: "AbpUsers",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "AbpUsers",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "AppBanner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LienKet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBanner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDanhMucCamNang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TitleSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDanhMucCamNang", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDanhMucChinhSach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TitleSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDanhMucChinhSach", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDanhMucSanPham",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AnhThumbnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhBanner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TitleSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDanhMucSanPham", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDonHang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiKhoanKhachHangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TongSoLuong = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDonHang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDonHang_AbpUsers_TaiKhoanKhachHangId",
                        column: x => x.TaiKhoanKhachHangId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppIdentitySettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentNumber = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppIdentitySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLienHe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DaXuLy = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLienHe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppQuaTang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppQuaTang", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppThuocTinh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppThuocTinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCamNang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Mota = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanhMucCamNangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TitleSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCamNang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCamNang_AppDanhMucCamNang_DanhMucCamNangId",
                        column: x => x.DanhMucCamNangId,
                        principalTable: "AppDanhMucCamNang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppChinhSach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DanhMucChinhSachId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChinhSach", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChinhSach_AppDanhMucChinhSach_DanhMucChinhSachId",
                        column: x => x.DanhMucChinhSachId,
                        principalTable: "AppDanhMucChinhSach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSanPham",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MoTaNgan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HuongDanSuDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThongSoKyThuat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GiaKhuyenMai = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DanhMucId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuaTangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    LuotMua = table.Column<int>(type: "int", nullable: false),
                    TitleSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionSEO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    JobId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaDatLich = table.Column<bool>(type: "bit", nullable: false),
                    ThoiHanBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiHanKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSanPham", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSanPham_AppDanhMucSanPham_DanhMucId",
                        column: x => x.DanhMucId,
                        principalTable: "AppDanhMucSanPham",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppSanPham_AppQuaTang_QuaTangId",
                        column: x => x.QuaTangId,
                        principalTable: "AppQuaTang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppGiaTriThuocTinh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThuocTinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGiaTriThuocTinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppGiaTriThuocTinh_AppThuocTinh_ThuocTinhId",
                        column: x => x.ThuocTinhId,
                        principalTable: "AppThuocTinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAnhSanPham",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAnhSanPham", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAnhSanPham_AppSanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "AppSanPham",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppChiTietDonHang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonHangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamBienThe = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    QuaTang = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GiamGiaVoucher = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChiTietDonHang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChiTietDonHang_AppDonHang_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "AppDonHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppChiTietDonHang_AppSanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "AppSanPham",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSanPhamBienThe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GiaKhuyenMai = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSanPhamBienThe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSanPhamBienThe_AppSanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "AppSanPham",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppVoucher",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaVoucher = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GiamGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LaPhanTram = table.Column<bool>(type: "bit", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonHangToiThieu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ThoiHanBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiHanKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    LaDatLich = table.Column<bool>(type: "bit", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppVoucher_AppSanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "AppSanPham",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSanPhamBienTheThuocTinh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamBienTheId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaTriThuocTinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSanPhamBienTheThuocTinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSanPhamBienTheThuocTinh_AppGiaTriThuocTinh_GiaTriThuocTinhId",
                        column: x => x.GiaTriThuocTinhId,
                        principalTable: "AppGiaTriThuocTinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppSanPhamBienTheThuocTinh_AppSanPhamBienThe_SanPhamBienTheId",
                        column: x => x.SanPhamBienTheId,
                        principalTable: "AppSanPhamBienThe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnhSanPham_SanPhamId",
                table: "AppAnhSanPham",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCamNang_DanhMucCamNangId",
                table: "AppCamNang",
                column: "DanhMucCamNangId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCamNang_Slug",
                table: "AppCamNang",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppChinhSach_DanhMucChinhSachId",
                table: "AppChinhSach",
                column: "DanhMucChinhSachId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChiTietDonHang_DonHangId",
                table: "AppChiTietDonHang",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChiTietDonHang_SanPhamId",
                table: "AppChiTietDonHang",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDanhMucCamNang_Slug",
                table: "AppDanhMucCamNang",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDanhMucChinhSach_Slug",
                table: "AppDanhMucChinhSach",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDanhMucSanPham_Slug",
                table: "AppDanhMucSanPham",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDonHang_TaiKhoanKhachHangId",
                table: "AppDonHang",
                column: "TaiKhoanKhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGiaTriThuocTinh_ThuocTinhId",
                table: "AppGiaTriThuocTinh",
                column: "ThuocTinhId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSanPham_DanhMucId",
                table: "AppSanPham",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSanPham_QuaTangId",
                table: "AppSanPham",
                column: "QuaTangId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSanPham_Slug",
                table: "AppSanPham",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSanPhamBienThe_SanPhamId",
                table: "AppSanPhamBienThe",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSanPhamBienTheThuocTinh_GiaTriThuocTinhId",
                table: "AppSanPhamBienTheThuocTinh",
                column: "GiaTriThuocTinhId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSanPhamBienTheThuocTinh_SanPhamBienTheId_GiaTriThuocTinhId",
                table: "AppSanPhamBienTheThuocTinh",
                columns: new[] { "SanPhamBienTheId", "GiaTriThuocTinhId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucher_MaVoucher",
                table: "AppVoucher",
                column: "MaVoucher",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucher_SanPhamId",
                table: "AppVoucher",
                column: "SanPhamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAnhSanPham");

            migrationBuilder.DropTable(
                name: "AppBanner");

            migrationBuilder.DropTable(
                name: "AppCamNang");

            migrationBuilder.DropTable(
                name: "AppChinhSach");

            migrationBuilder.DropTable(
                name: "AppChiTietDonHang");

            migrationBuilder.DropTable(
                name: "AppIdentitySettings");

            migrationBuilder.DropTable(
                name: "AppLienHe");

            migrationBuilder.DropTable(
                name: "AppSanPhamBienTheThuocTinh");

            migrationBuilder.DropTable(
                name: "AppVoucher");

            migrationBuilder.DropTable(
                name: "AppDanhMucCamNang");

            migrationBuilder.DropTable(
                name: "AppDanhMucChinhSach");

            migrationBuilder.DropTable(
                name: "AppDonHang");

            migrationBuilder.DropTable(
                name: "AppGiaTriThuocTinh");

            migrationBuilder.DropTable(
                name: "AppSanPhamBienThe");

            migrationBuilder.DropTable(
                name: "AppThuocTinh");

            migrationBuilder.DropTable(
                name: "AppSanPham");

            migrationBuilder.DropTable(
                name: "AppDanhMucSanPham");

            migrationBuilder.DropTable(
                name: "AppQuaTang");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "IsCustomer",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AbpUsers");
        }
    }
}
