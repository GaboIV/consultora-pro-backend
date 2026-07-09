using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsultoraPro.Application.Interfaces;

/// <summary>
/// Abstracción de almacenamiento de archivos. Cada implementación (filesystem local en dev,
/// Azure Blob en QA/Prod) persiste el contenido y expone una <b>key relativa</b> estable que
/// se guarda en la BD (no la URL absoluta). La URL de acceso se resuelve en tiempo de lectura
/// vía <see cref="GetAccessUrlAsync"/> para poder firmarla con expiración corta.
/// </summary>
public interface IStorageService
{
    /// <summary>Persiste el contenido y devuelve la key relativa (ej. "screenshots/{guid}.png").</summary>
    /// <param name="category">Prefijo lógico/carpeta: "screenshots", "adjuntos", "inline".</param>
    Task<StoredFile> SaveFileAsync(Stream content, string fileName, string contentType, string category);

    /// <summary>
    /// URL de acceso de corta expiración para la key dada: SAS firmado en Azure, URL estática en Local.
    /// </summary>
    Task<string> GetAccessUrlAsync(string key, TimeSpan? expiry = null);

    /// <summary>
    /// Abre el contenido de la key para lectura server-side (ej. empaquetar un ZIP). El llamador
    /// es responsable de liberar el stream. Devuelve <c>null</c> si la key no existe.
    /// </summary>
    Task<Stream?> OpenReadAsync(string key);

    /// <summary>Elimina el archivo asociado a la key. No falla si la key no existe.</summary>
    Task DeleteFileAsync(string key);

    /// <summary>
    /// Intenta extraer la key relativa desde una URL de acceso emitida por este proveedor
    /// (estática o SAS). Permite normalizar a placeholders el contenido que se persiste.
    /// </summary>
    bool TryGetKeyFromUrl(string url, out string key);
}

/// <summary>Resultado de una subida: key relativa a persistir + metadatos.</summary>
public record StoredFile(string Key, string ContentType, long SizeBytes);
