using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Nueva tabla de checklists (agrupadores con nombre).
            migrationBuilder.CreateTable(
                name: "Checklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TarjetaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checklists_Tarjetas_TarjetaId",
                        column: x => x.TarjetaId,
                        principalTable: "Tarjetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_TarjetaId",
                table: "Checklists",
                column: "TarjetaId");

            // 2. Preservar datos: crear un checklist por defecto "Checklist" para cada tarjeta
            //    que ya tenga ítems sueltos (modelo plano anterior).
            migrationBuilder.Sql(
                "INSERT INTO `Checklists` (`Id`, `TarjetaId`, `Nombre`, `Orden`) " +
                "SELECT UUID(), `TarjetaId`, 'Checklist', 65536 " +
                "FROM `ChecklistItems` GROUP BY `TarjetaId`;");

            // 3. Reparentar los ítems: de Tarjeta a Checklist.
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistItems_Tarjetas_TarjetaId",
                table: "ChecklistItems");

            migrationBuilder.RenameColumn(
                name: "TarjetaId",
                table: "ChecklistItems",
                newName: "ChecklistId");

            // En este punto ChecklistId conserva el TarjetaId original; lo traducimos al Id del
            // checklist por defecto recién creado para esa tarjeta.
            migrationBuilder.Sql(
                "UPDATE `ChecklistItems` ci " +
                "JOIN `Checklists` c ON ci.`ChecklistId` = c.`TarjetaId` " +
                "SET ci.`ChecklistId` = c.`Id`;");

            migrationBuilder.RenameIndex(
                name: "IX_ChecklistItems_TarjetaId",
                table: "ChecklistItems",
                newName: "IX_ChecklistItems_ChecklistId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItems_Checklists_ChecklistId",
                table: "ChecklistItems",
                column: "ChecklistId",
                principalTable: "Checklists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistItems_Checklists_ChecklistId",
                table: "ChecklistItems");

            migrationBuilder.RenameColumn(
                name: "ChecklistId",
                table: "ChecklistItems",
                newName: "TarjetaId");

            // Devolver cada ítem a su tarjeta original (ChecklistId → Checklist.TarjetaId).
            migrationBuilder.Sql(
                "UPDATE `ChecklistItems` ci " +
                "JOIN `Checklists` c ON ci.`TarjetaId` = c.`Id` " +
                "SET ci.`TarjetaId` = c.`TarjetaId`;");

            migrationBuilder.RenameIndex(
                name: "IX_ChecklistItems_ChecklistId",
                table: "ChecklistItems",
                newName: "IX_ChecklistItems_TarjetaId");

            migrationBuilder.DropTable(
                name: "Checklists");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItems_Tarjetas_TarjetaId",
                table: "ChecklistItems",
                column: "TarjetaId",
                principalTable: "Tarjetas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
