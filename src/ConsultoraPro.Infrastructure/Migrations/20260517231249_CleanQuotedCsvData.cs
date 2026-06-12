using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanQuotedCsvData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE `AmbienteCloudResources` SET
                    `NombreRecurso` = TRIM(BOTH '"' FROM `NombreRecurso`),
                    `TipoRecurso`   = TRIM(BOTH '"' FROM `TipoRecurso`),
                    `Ubicacion`     = TRIM(BOTH '"' FROM `Ubicacion`),
                    `DeepLink`      = TRIM(BOTH '"' FROM `DeepLink`),
                    `Nota`          = TRIM(BOTH '"' FROM `Nota`),
                    `Plataforma`    = TRIM(BOTH '"' FROM `Plataforma`)
                WHERE `NombreRecurso` LIKE '"%'
                   OR `TipoRecurso`   LIKE '"%'
                   OR `Ubicacion`     LIKE '"%'
                   OR `DeepLink`      LIKE '"%'
                   OR `Nota`          LIKE '"%'
                   OR `Plataforma`    LIKE '"%'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
