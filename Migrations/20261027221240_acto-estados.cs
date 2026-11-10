using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActosAdultosAPI.Migrations
{
    /// <inheritdoc />
    public partial class actoestados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsActivo",
                table: "Actos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteCargaCargos",
                table: "Actos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsActivo",
                table: "Actos");

            migrationBuilder.DropColumn(
                name: "PermiteCargaCargos",
                table: "Actos");
        }
    }
}
