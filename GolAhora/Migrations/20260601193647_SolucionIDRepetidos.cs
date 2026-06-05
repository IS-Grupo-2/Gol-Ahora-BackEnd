using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class SolucionIDRepetidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ClientProfiles_clientidClient",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Discounts_discountidDiscount",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_clientidClient",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_discountidDiscount",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_idClient",
                table: "Payments",
                column: "idClient");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_idDiscount",
                table: "Payments",
                column: "idDiscount");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ClientProfiles_idClient",
                table: "Payments",
                column: "idClient",
                principalTable: "ClientProfiles",
                principalColumn: "idClient",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Discounts_idDiscount",
                table: "Payments",
                column: "idDiscount",
                principalTable: "Discounts",
                principalColumn: "idDiscount",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ClientProfiles_idClient",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Discounts_idDiscount",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_idClient",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_idDiscount",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "clientidClient",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "discountidDiscount",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_clientidClient",
                table: "Payments",
                column: "clientidClient");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_discountidDiscount",
                table: "Payments",
                column: "discountidDiscount");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ClientProfiles_clientidClient",
                table: "Payments",
                column: "clientidClient",
                principalTable: "ClientProfiles",
                principalColumn: "idClient",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Discounts_discountidDiscount",
                table: "Payments",
                column: "discountidDiscount",
                principalTable: "Discounts",
                principalColumn: "idDiscount");
        }
    }
}