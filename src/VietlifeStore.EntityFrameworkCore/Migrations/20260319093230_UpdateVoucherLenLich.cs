using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucherLenLich : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLichVoucher");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "AppVoucher");

            migrationBuilder.RenameColumn(
                name: "SoLuong",
                table: "AppVoucher",
                newName: "PhamVi");

            migrationBuilder.RenameColumn(
                name: "LaDatLich",
                table: "AppVoucher",
                newName: "ChiPhatHanhCuThe");

            migrationBuilder.AddColumn<decimal>(
                name: "GiaTriGiam",
                table: "AppVoucherDaSuDung",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "TrangThai",
                table: "AppVoucher",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "DonHangToiThieu",
                table: "AppVoucher",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "DaDung",
                table: "AppVoucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GiamToiDa",
                table: "AppVoucher",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GioiHanMoiUser",
                table: "AppVoucher",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "HangfireActivateJobId",
                table: "AppVoucher",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HangfireExpireJobId",
                table: "AppVoucher",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LoaiVoucher",
                table: "AppVoucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "AppVoucher",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenVoucher",
                table: "AppVoucher",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TongSoLuong",
                table: "AppVoucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppVoucherDoiTuong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiDoiTuong = table.Column<int>(type: "int", nullable: false),
                    DoiTuongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_AppVoucherDoiTuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppVoucherDoiTuong_AppVoucher_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "AppVoucher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppVoucherNguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuongNhan = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DaDung = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NgayNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaHetHan = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_AppVoucherNguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppVoucherNguoiDung_AppVoucher_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "AppVoucher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppVoucherSchedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HangfireJobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiJob = table.Column<int>(type: "int", nullable: false),
                    ThoiGianDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianThucThi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                    table.PrimaryKey("PK_AppVoucherSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppVoucherSchedule_AppVoucher_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "AppVoucher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucherDaSuDung_DonHangId",
                table: "AppVoucherDaSuDung",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucherDaSuDung_VoucherId_UserId",
                table: "AppVoucherDaSuDung",
                columns: new[] { "VoucherId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucherDoiTuong_VoucherId_LoaiDoiTuong_DoiTuongId",
                table: "AppVoucherDoiTuong",
                columns: new[] { "VoucherId", "LoaiDoiTuong", "DoiTuongId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucherNguoiDung_VoucherId_UserId",
                table: "AppVoucherNguoiDung",
                columns: new[] { "VoucherId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucherSchedule_HangfireJobId",
                table: "AppVoucherSchedule",
                column: "HangfireJobId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucherSchedule_VoucherId_LoaiJob_TrangThai",
                table: "AppVoucherSchedule",
                columns: new[] { "VoucherId", "LoaiJob", "TrangThai" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppVoucherDaSuDung_AppVoucher_VoucherId",
                table: "AppVoucherDaSuDung",
                column: "VoucherId",
                principalTable: "AppVoucher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppVoucherDaSuDung_AppVoucher_VoucherId",
                table: "AppVoucherDaSuDung");

            migrationBuilder.DropTable(
                name: "AppVoucherDoiTuong");

            migrationBuilder.DropTable(
                name: "AppVoucherNguoiDung");

            migrationBuilder.DropTable(
                name: "AppVoucherSchedule");

            migrationBuilder.DropIndex(
                name: "IX_AppVoucherDaSuDung_DonHangId",
                table: "AppVoucherDaSuDung");

            migrationBuilder.DropIndex(
                name: "IX_AppVoucherDaSuDung_VoucherId_UserId",
                table: "AppVoucherDaSuDung");

            migrationBuilder.DropColumn(
                name: "GiaTriGiam",
                table: "AppVoucherDaSuDung");

            migrationBuilder.DropColumn(
                name: "DaDung",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "GiamToiDa",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "GioiHanMoiUser",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "HangfireActivateJobId",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "HangfireExpireJobId",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "LoaiVoucher",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "TenVoucher",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "TongSoLuong",
                table: "AppVoucher");

            migrationBuilder.RenameColumn(
                name: "PhamVi",
                table: "AppVoucher",
                newName: "SoLuong");

            migrationBuilder.RenameColumn(
                name: "ChiPhatHanhCuThe",
                table: "AppVoucher",
                newName: "LaDatLich");

            migrationBuilder.AlterColumn<bool>(
                name: "TrangThai",
                table: "AppVoucher",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "DonHangToiThieu",
                table: "AppVoucher",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "AppVoucher",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppLichVoucher",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodePrefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonHangToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiaTriGocSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaTriMoi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaTriTaoMoi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LaPhanTramMoi = table.Column<bool>(type: "bit", nullable: true),
                    LaPhanTramSnapshot = table.Column<bool>(type: "bit", nullable: true),
                    LaPhanTramTaoMoi = table.Column<bool>(type: "bit", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoaiHanhDong = table.Column<int>(type: "int", maxLength: 20, nullable: false, defaultValue: 0),
                    MaxUsageMoiVoucher = table.Column<int>(type: "int", nullable: true),
                    NgayHieuLuc = table.Column<int>(type: "int", nullable: true),
                    SoLuongTao = table.Column<int>(type: "int", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", maxLength: 20, nullable: false, defaultValue: 0),
                    TrangThaiSnapshot = table.Column<bool>(type: "bit", nullable: true),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLichVoucher", x => x.Id);
                });

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
    }
}
