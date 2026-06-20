using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalTableros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tableros_ProyectoId_Clave",
                table: "Tableros");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProyectoId",
                table: "Tableros",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorId",
                table: "Tableros",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Tableros_CreadoPorId_Activo",
                table: "Tableros",
                columns: new[] { "CreadoPorId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Tableros_ProyectoId_Clave",
                table: "Tableros",
                columns: new[] { "ProyectoId", "Clave" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tableros_AspNetUsers_CreadoPorId",
                table: "Tableros",
                column: "CreadoPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tableros_AspNetUsers_CreadoPorId",
                table: "Tableros");

            migrationBuilder.DropIndex(
                name: "IX_Tableros_CreadoPorId_Activo",
                table: "Tableros");

            migrationBuilder.DropIndex(
                name: "IX_Tableros_ProyectoId_Clave",
                table: "Tableros");

            migrationBuilder.DropColumn(
                name: "CreadoPorId",
                table: "Tableros");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProyectoId",
                table: "Tableros",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Tableros_ProyectoId_Clave",
                table: "Tableros",
                columns: new[] { "ProyectoId", "Clave" },
                unique: true);
        }
    }
}
