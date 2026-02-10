using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppVoucher_AppSanPham_SanPhamId",
                table: "AppVoucher");

            migrationBuilder.DropIndex(
                name: "IX_AppVoucher_SanPhamId",
                table: "AppVoucher");

            migrationBuilder.DropColumn(
                name: "SanPhamId",
                table: "AppVoucher");

            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "AppVoucher",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "AppVoucher",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SanPhamId",
                table: "AppVoucher",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppVoucher_SanPhamId",
                table: "AppVoucher",
                column: "SanPhamId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppVoucher_AppSanPham_SanPhamId",
                table: "AppVoucher",
                column: "SanPhamId",
                principalTable: "AppSanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
