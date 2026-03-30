using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class AddDatLich : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppLichGiamGia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BienTheId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoaiGiam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaGocSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaGocBienTheSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppLichGiamGia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLichVoucher",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoaiHanhDong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activate"),
                    GiaTriMoi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LaPhanTramMoi = table.Column<bool>(type: "bit", nullable: true),
                    GiaTriGocSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LaPhanTramSnapshot = table.Column<bool>(type: "bit", nullable: true),
                    TrangThaiSnapshot = table.Column<bool>(type: "bit", nullable: true),
                    CodePrefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoLuongTao = table.Column<int>(type: "int", nullable: true),
                    MaxUsageMoiVoucher = table.Column<int>(type: "int", nullable: true),
                    NgayHieuLuc = table.Column<int>(type: "int", nullable: true),
                    DonHangToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    GiaTriTaoMoi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LaPhanTramTaoMoi = table.Column<bool>(type: "bit", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppLichVoucher", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLichGiamGia_BienTheId",
                table: "AppLichGiamGia",
                column: "BienTheId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLichGiamGia_SanPhamId",
                table: "AppLichGiamGia",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLichGiamGia_ThoiGianBatDau_ThoiGianKetThuc",
                table: "AppLichGiamGia",
                columns: new[] { "ThoiGianBatDau", "ThoiGianKetThuc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLichGiamGia_TrangThai",
                table: "AppLichGiamGia",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_AppLichVoucher_ThoiGianBatDau_ThoiGianKetThuc",
                table: "AppLichVoucher",
                columns: new[] { "ThoiGianBatDau", "ThoiGianKetThuc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLichVoucher_TrangThai",
                table: "AppLichVoucher",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_AppLichVoucher_VoucherId",
                table: "AppLichVoucher",
                column: "VoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLichGiamGia");

            migrationBuilder.DropTable(
                name: "AppLichVoucher");
        }
    }
}
