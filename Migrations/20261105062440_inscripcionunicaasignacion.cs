using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActosAdultosAPI.Migrations
{
    /// <inheritdoc />
    public partial class inscripcionunicaasignacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_CargoId",
                table: "Inscripciones",
                column: "CargoId",
                unique: true,
                filter: "[Eliminado] = 0 AND [EstadoInscripcion] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_CargoId",
                table: "Inscripciones");
        }
    }
}
