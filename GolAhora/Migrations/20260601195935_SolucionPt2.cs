using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class SolucionPt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Payments_paymentidPayment",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_paymentidPayment",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "paymentidPayment",
                table: "Receipts");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_idPayment",
                table: "Receipts",
                column: "idPayment",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Payments_idPayment",
                table: "Receipts",
                column: "idPayment",
                principalTable: "Payments",
                principalColumn: "idPayment",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Payments_idPayment",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_idPayment",
                table: "Receipts");

            migrationBuilder.AddColumn<int>(
                name: "paymentidPayment",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_paymentidPayment",
                table: "Receipts",
                column: "paymentidPayment");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Payments_paymentidPayment",
                table: "Receipts",
                column: "paymentidPayment",
                principalTable: "Payments",
                principalColumn: "idPayment",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
