using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFileUrlToStorageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Se reemplaza la columna en lugar de renombrarla a propósito: los valores antiguos eran
            // URLs absolutas (p.ej. "https://host/uploads/{guid}.png"), no keys relativas. Conservarlos
            // produciría keys inválidas para el nuevo esquema, así que se descartan (sin datos productivos).
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Screenshots");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "AdjuntosTarjeta");

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Screenshots",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "AdjuntosTarjeta",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Screenshots");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "AdjuntosTarjeta");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Screenshots",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "AdjuntosTarjeta",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
