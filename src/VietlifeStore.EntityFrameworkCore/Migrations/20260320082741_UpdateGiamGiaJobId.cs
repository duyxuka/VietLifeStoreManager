using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGiamGiaJobId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobId",
                table: "AppSanPham");

            migrationBuilder.DropColumn(
                name: "LaDatLich",
                table: "AppSanPham");

            migrationBuilder.DropColumn(
                name: "ThoiHanBatDau",
                table: "AppSanPham");

            migrationBuilder.DropColumn(
                name: "ThoiHanKetThuc",
                table: "AppSanPham");

            migrationBuilder.AddColumn<string>(
                name: "ActivateJobId",
                table: "AppChuongTrinhGiamGia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpireJobId",
                table: "AppChuongTrinhGiamGia",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivateJobId",
                table: "AppChuongTrinhGiamGia");

            migrationBuilder.DropColumn(
                name: "ExpireJobId",
                table: "AppChuongTrinhGiamGia");

            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "AppSanPham",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LaDatLich",
                table: "AppSanPham",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiHanBatDau",
                table: "AppSanPham",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiHanKetThuc",
                table: "AppSanPham",
                type: "datetime2",
                nullable: true);
        }
    }
}
