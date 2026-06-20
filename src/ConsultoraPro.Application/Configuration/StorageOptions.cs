using System.Collections.Generic;

namespace ConsultoraPro.Application.Configuration;

/// <summary>
/// Configuración del subsistema de almacenamiento de archivos (sección "Storage").
/// El proveedor activo se elige por <see cref="Provider"/>; los límites se centralizan aquí
/// en lugar de constantes dispersas en los controllers.
/// </summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>"Local" (filesystem) o "AzureBlob".</summary>
    public string Provider { get; set; } = "Local";

    /// <summary>Expiración por defecto de las URLs firmadas (SAS).</summary>
    public int SignedUrlExpiryMinutes { get; set; } = 15;

    public LocalStorageOptions Local { get; set; } = new();
    public AzureBlobOptions AzureBlob { get; set; } = new();
    public StorageLimits Limits { get; set; } = new();
}

public class LocalStorageOptions
{
    /// <summary>Raíz física de los archivos (ej. "D:\\ConsultoraPro\\uploads"). Vacío => {cwd}/uploads.</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Base pública para componer la URL estática servida por /uploads.</summary>
    public string PublicBaseUrl { get; set; } = "https://localhost:7001";
}

public class AzureBlobOptions
{
    /// <summary>Connection string con account key (permite firmar SAS). Inyectada como secreto.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Contenedor privado por entorno (ej. "consultorapro-qa").</summary>
    public string ContainerName { get; set; } = string.Empty;
}

public class StorageLimits
{
    public long MaxImageBytes { get; set; } = 5 * 1024 * 1024;       // 5 MB
    public long MaxAttachmentBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

    public List<string> AllowedImageExtensions { get; set; } =
        new() { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
}
