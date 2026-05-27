using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class RenamePersonalClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "specialty",
                table: "ProfessorProfiles",
                newName: "speciality");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "speciality",
                table: "ProfessorProfiles",
                newName: "specialty");
        }
    }
}
