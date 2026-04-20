using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideoSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppSocialVideo");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "AppSocialVideo");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "AppSocialVideo");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "AppSocialVideo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppSocialVideo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "AppSocialVideo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "AppSocialVideo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "AppSocialVideo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
