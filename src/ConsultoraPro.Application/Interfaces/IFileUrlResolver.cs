using System.Threading.Tasks;

namespace ConsultoraPro.Application.Interfaces;

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
}
