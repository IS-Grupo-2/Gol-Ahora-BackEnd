using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class ArregloBaseDatosMilena : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
