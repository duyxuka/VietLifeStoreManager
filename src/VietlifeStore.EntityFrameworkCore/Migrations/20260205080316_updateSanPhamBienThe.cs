using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class updateSanPhamBienThe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ten",
                table: "AppSanPhamBienThe",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ten",
                table: "AppSanPhamBienThe");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);
        }
    }
}
