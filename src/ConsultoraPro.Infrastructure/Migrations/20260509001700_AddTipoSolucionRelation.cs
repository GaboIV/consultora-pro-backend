using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoSolucionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TipoSolucionId",
                table: "Proyectos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "TiposSolucion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposSolucion", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_TipoSolucionId",
                table: "Proyectos",
                column: "TipoSolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposSolucion_Nombre",
                table: "TiposSolucion",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_TiposSolucion_TipoSolucionId",
                table: "Proyectos",
                column: "TipoSolucionId",
                principalTable: "TiposSolucion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_TiposSolucion_TipoSolucionId",
                table: "Proyectos");

            migrationBuilder.DropTable(
                name: "TiposSolucion");

            migrationBuilder.DropIndex(
                name: "IX_Proyectos_TipoSolucionId",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "TipoSolucionId",
                table: "Proyectos");
        }
    }
}
