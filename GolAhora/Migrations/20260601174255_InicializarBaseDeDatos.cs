using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class InicializarBaseDeDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "foulsTeamLocal",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "foulsTeamVisitor",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "observations",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "penaltiesTeamLocal",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "penaltiesTeamVisitor",
                table: "Results",
                newName: "idMatch");

            migrationBuilder.AddColumn<DateTime>(
                name: "date",
                table: "Assistances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date",
                table: "Assistances");

            migrationBuilder.RenameColumn(
                name: "idMatch",
                table: "Results",
                newName: "penaltiesTeamVisitor");

            migrationBuilder.AddColumn<int>(
                name: "foulsTeamLocal",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "foulsTeamVisitor",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "observations",
                table: "Results",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "penaltiesTeamLocal",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
