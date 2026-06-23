using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoSolucionActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "TiposSolucion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "TiposSolucion");
        }
    }
}
