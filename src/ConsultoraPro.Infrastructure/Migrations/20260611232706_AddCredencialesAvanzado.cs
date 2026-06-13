using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCredencialesAvanzado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CamposExtra",
                table: "Credenciales",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecretosExtraCifrado",
                table: "Credenciales",
                type: "text",
                maxLength: null,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Accion",
                table: "AuditoriasCredenciales",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Lectura")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Detalle",
                table: "AuditoriasCredenciales",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CamposExtra",
                table: "Credenciales");

            migrationBuilder.DropColumn(
                name: "SecretosExtraCifrado",
                table: "Credenciales");

            migrationBuilder.DropColumn(
                name: "Accion",
                table: "AuditoriasCredenciales");

            migrationBuilder.DropColumn(
                name: "Detalle",
                table: "AuditoriasCredenciales");
        }
    }
}
