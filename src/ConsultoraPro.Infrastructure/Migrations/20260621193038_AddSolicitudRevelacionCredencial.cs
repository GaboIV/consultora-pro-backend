using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudRevelacionCredencial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesRevelacionCredencial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CredencialId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SolicitanteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AprobadorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotaResolucion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    FechaResolucion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VigenteHasta = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesRevelacionCredencial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesRevelacionCredencial_AspNetUsers_AprobadorId",
                        column: x => x.AprobadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesRevelacionCredencial_AspNetUsers_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesRevelacionCredencial_Credenciales_CredencialId",
                        column: x => x.CredencialId,
                        principalTable: "Credenciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRevelacionCredencial_AprobadorId",
                table: "SolicitudesRevelacionCredencial",
                column: "AprobadorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRevelacionCredencial_CredencialId_SolicitanteId_E~",
                table: "SolicitudesRevelacionCredencial",
                columns: new[] { "CredencialId", "SolicitanteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRevelacionCredencial_SolicitanteId",
                table: "SolicitudesRevelacionCredencial",
                column: "SolicitanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesRevelacionCredencial");
        }
    }
}
