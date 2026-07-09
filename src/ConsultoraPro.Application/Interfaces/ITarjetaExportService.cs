using System;
using System.Threading.Tasks;

namespace ConsultoraPro.Application.Interfaces;

/// <summary>Contenido binario de un ZIP de exportación listo para descargar.</summary>
/// <param name="Contenido">Bytes del archivo ZIP.</param>
/// <param name="NombreArchivo">Nombre sugerido de descarga (ej. "P2P-INC-008.zip").</param>
public record TarjetaExportResult(byte[] Contenido, string NombreArchivo);

/// <summary>
/// Empaqueta todo el contexto de una tarjeta (descripción, checklist, datos generales,
/// comentarios, actividad, imágenes inline y adjuntos) en un ZIP.
/// </summary>
public interface ITarjetaExportService
{
    /// <summary>
    /// Genera el ZIP de la tarjeta. Devuelve <c>null</c> si la tarjeta no existe o está inactiva.
    /// Lanza <see cref="UnauthorizedAccessException"/> si el usuario no puede acceder al tablero.
    /// </summary>
    Task<TarjetaExportResult?> ExportarAsync(Guid tarjetaId);
}
