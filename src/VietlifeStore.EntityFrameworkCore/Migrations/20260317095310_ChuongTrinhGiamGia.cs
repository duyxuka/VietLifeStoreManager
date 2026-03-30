using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class ChuongTrinhGiamGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLichGiamGia");

            migrationBuilder.AlterColumn<int>(
                name: "TrangThai",
                table: "AppLichVoucher",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<int>(
                name: "LoaiHanhDong",
                table: "AppLichVoucher",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Activate");

            migrationBuilder.CreateTable(
                name: "AppChuongTrinhGiamGia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenChuongTrinh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppChuongTrinhGiamGia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppChuongTrinhGiamGiaItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChuongTrinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BienTheId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GiaSauGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaGocSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaGocBienTheSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QuaTangId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChuongTrinhGiamGiaItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChuongTrinhGiamGiaItem_AppChuongTrinhGiamGia_ChuongTrinhId",
                        column: x => x.ChuongTrinhId,
                        principalTable: "AppChuongTrinhGiamGia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppChuongTrinhGiamGiaItem_AppQuaTang_QuaTangId",
                        column: x => x.QuaTangId,
                        principalTable: "AppQuaTang",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppChuongTrinhGiamGiaItem_AppSanPhamBienThe_BienTheId",
                        column: x => x.BienTheId,
                        principalTable: "AppSanPhamBienThe",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppChuongTrinhGiamGiaItem_AppSanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "AppSanPham",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppChuongTrinhGiamGia_TrangThai_ThoiGianBatDau_ThoiGianKetThuc",
                table: "AppChuongTrinhGiamGia",
                columns: new[] { "TrangThai", "ThoiGianBatDau", "ThoiGianKetThuc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppChuongTrinhGiamGiaItem_BienTheId",
                table: "AppChuongTrinhGiamGiaItem",
                column: "BienTheId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChuongTrinhGiamGiaItem_ChuongTrinhId_SanPhamId_BienTheId",
                table: "AppChuongTrinhGiamGiaItem",
                columns: new[] { "ChuongTrinhId", "SanPhamId", "BienTheId" },
                unique: true,
                filter: "[SanPhamId] IS NOT NULL AND [BienTheId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppChuongTrinhGiamGiaItem_QuaTangId",
                table: "AppChuongTrinhGiamGiaItem",
                column: "QuaTangId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChuongTrinhGiamGiaItem_SanPhamId",
                table: "AppChuongTrinhGiamGiaItem",
                column: "SanPhamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppChuongTrinhGiamGiaItem");

            migrationBuilder.DropTable(
                name: "AppChuongTrinhGiamGia");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "AppLichVoucher",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 20,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "LoaiHanhDong",
                table: "AppLichVoucher",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Activate",
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 20,
                oldDefaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppLichGiamGia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BienTheId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiaGocBienTheSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaGocSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoaiGiam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLichGiamGia", x => x.Id);
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
        }
    }
}
