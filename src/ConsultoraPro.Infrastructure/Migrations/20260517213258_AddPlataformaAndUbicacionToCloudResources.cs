using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlataformaAndUbicacionToCloudResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plataforma",
                table: "AmbienteCloudResources",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Azure")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "AmbienteCloudResources",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plataforma",
                table: "AmbienteCloudResources");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "AmbienteCloudResources");
        }
    }
}
