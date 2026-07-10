using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class ReservationStatusForFrontendFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "createdAt",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "pendiente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Reservations");
        }
    }
}
