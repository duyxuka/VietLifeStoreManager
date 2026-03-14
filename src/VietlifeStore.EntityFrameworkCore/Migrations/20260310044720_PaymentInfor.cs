using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietlifeStore.Migrations
{
    /// <inheritdoc />
    public partial class PaymentInfor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInformationModels",
                table: "PaymentInformationModels");

            migrationBuilder.RenameTable(
                name: "PaymentInformationModels",
                newName: "AppPaymentInformationModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppPaymentInformationModel",
                table: "AppPaymentInformationModel",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppPaymentInformationModel",
                table: "AppPaymentInformationModel");

            migrationBuilder.RenameTable(
                name: "AppPaymentInformationModel",
                newName: "PaymentInformationModels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInformationModels",
                table: "PaymentInformationModels",
                column: "Id");
        }
    }
}
