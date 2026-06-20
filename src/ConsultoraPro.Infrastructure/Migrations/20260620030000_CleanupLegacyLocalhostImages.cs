using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultoraPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLegacyLocalhostImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Limpieza de datos legacy: descripciones y comentarios guardados ANTES del refactor de
            // almacenamiento contienen imágenes inline cuyo origen apunta al servidor de desarrollo
            // (p.ej. https://localhost:7001/uploads/inline/{guid}.png). Esos bytes solo existieron en el
            // disco de dev (nunca en Azure Blob), por lo que las referencias están muertas en QA/Prod y
            // el navegador devuelve ERR_CONNECTION_REFUSED. Se eliminan las referencias <img> y la
            // sintaxis markdown de imagen que apunten a localhost, dejando el texto limpio.
            //
            // Nota: MySQL procesa '\\' en el literal SQL como '\', de modo que REGEXP_REPLACE recibe
            // las secuencias de escape correctas (\[ \] \( \) literales en el patrón markdown).

            // 1) Tarjetas.Descripcion — <img ...localhost...>
            migrationBuilder.Sql(
                "UPDATE `Tarjetas` SET `Descripcion` = " +
                "REGEXP_REPLACE(`Descripcion`, '<img[^>]*localhost[^>]*>', '') " +
                "WHERE `Descripcion` LIKE '%localhost%';");

            // 2) Tarjetas.Descripcion — markdown ![alt](...localhost...)
            migrationBuilder.Sql(
                "UPDATE `Tarjetas` SET `Descripcion` = " +
                "REGEXP_REPLACE(`Descripcion`, '!\\\\[[^\\\\]]*\\\\]\\\\([^\\\\)]*localhost[^\\\\)]*\\\\)', '') " +
                "WHERE `Descripcion` LIKE '%localhost%';");

            // 3) ComentariosTarjeta.Texto — <img ...localhost...>
            migrationBuilder.Sql(
                "UPDATE `ComentariosTarjeta` SET `Texto` = " +
                "REGEXP_REPLACE(`Texto`, '<img[^>]*localhost[^>]*>', '') " +
                "WHERE `Texto` LIKE '%localhost%';");

            // 4) ComentariosTarjeta.Texto — markdown ![alt](...localhost...)
            migrationBuilder.Sql(
                "UPDATE `ComentariosTarjeta` SET `Texto` = " +
                "REGEXP_REPLACE(`Texto`, '!\\\\[[^\\\\]]*\\\\]\\\\([^\\\\)]*localhost[^\\\\)]*\\\\)', '') " +
                "WHERE `Texto` LIKE '%localhost%';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Migración de datos irreversible: las referencias eliminadas apuntaban a bytes inexistentes.
        }
    }
}
