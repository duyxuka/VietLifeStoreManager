using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiamGiaVoucher",
                table: "AppChiTietDonHang");

            migrationBuilder.AddColumn<decimal>(
                name: "GiamGiaVoucher",
                table: "AppDonHang",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiamGiaVoucher",
                table: "AppDonHang");

            migrationBuilder.AddColumn<decimal>(
                name: "GiamGiaVoucher",
                table: "AppChiTietDonHang",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
