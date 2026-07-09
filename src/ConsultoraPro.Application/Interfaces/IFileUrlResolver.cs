using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsultoraPro.Application.Interfaces;

/// <summary>Referencia a una imagen de almacenamiento propio embebida en un contenido.</summary>
/// <param name="Placeholder">Texto exacto a reemplazar en el contenido (ej. "cpfile://inline/abc.png").</param>
/// <param name="Key">Key relativa en el almacenamiento (ej. "inline/abc.png").</param>
public readonly record struct StorageImageRef(string Placeholder, string Key);

/// <summary>
/// Resuelve, en tiempo de lectura, las keys de almacenamiento persistidas en BD a URLs de acceso
/// firmadas (SAS) de corta expiración. Centraliza la firma para no acoplar los mappers al
/// <see cref="IStorageService"/> ni hacerlos asíncronos.
/// </summary>
public interface IFileUrlResolver
{
    /// <summary>Convierte una key (o un placeholder "cpfile://{key}") en una URL de acceso firmada.</summary>
    Task<string?> ResolveAsync(string? keyOrPlaceholder);

    /// <summary>
    /// Reemplaza dentro de un contenido HTML/markdown cada placeholder "cpfile://{key}" por una URL
    /// de acceso firmada y fresca. Usado para imágenes inline embebidas en descripciones.
    /// </summary>
    Task<string?> ResolveContentAsync(string? content);

    /// <summary>
    /// Operación inversa, aplicada al PERSISTIR: convierte las URLs de imagen que apuntan a nuestro
    /// almacenamiento (estáticas o SAS) en placeholders estables "cpfile://{key}", para que el
    /// contenido guardado no contenga URLs firmadas que expiran.
    /// </summary>
    string? ToStoragePlaceholders(string? content);

    /// <summary>
    /// Extrae las imágenes de almacenamiento propio embebidas en un contenido markdown/HTML
    /// (normalizando antes URLs legacy a placeholders). Usado por el export para materializar
    /// cada imagen como archivo y reescribir su enlace a una ruta relativa dentro del ZIP.
    /// </summary>
    IReadOnlyList<StorageImageRef> ExtractStorageImages(string? content);
}
