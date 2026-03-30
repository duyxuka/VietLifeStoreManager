using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQueueandLogMail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaMo",
                table: "AppEmailQueue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TenKhachHang",
                table: "AppEmailQueue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianMo",
                table: "AppEmailQueue",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DaMo",
                table: "AppEmailLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianMo",
                table: "AppEmailLog",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaMo",
                table: "AppEmailQueue");

            migrationBuilder.DropColumn(
                name: "TenKhachHang",
                table: "AppEmailQueue");

            migrationBuilder.DropColumn(
                name: "ThoiGianMo",
                table: "AppEmailQueue");

            migrationBuilder.DropColumn(
                name: "DaMo",
                table: "AppEmailLog");

            migrationBuilder.DropColumn(
                name: "ThoiGianMo",
                table: "AppEmailLog");
        }
    }
}
