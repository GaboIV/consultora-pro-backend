namespace ConsultoraPro.Domain.Models;

/// <summary>Archivo concreto de una versión de <see cref="Documento"/>. Inmutable una vez subido.</summary>
public class DocumentoVersion
{
    public Guid Id { get; set; }
    public Guid DocumentoId { get; set; }
    public Documento Documento { get; set; } = null!;
    /// <summary>Correlativo interno (1, 2, 3…) que ordena el histórico.</summary>
    public int Numero { get; set; }
    /// <summary>Etiqueta de versión visible (ej. "1.0", "2.1").</summary>
    public string Version { get; set; } = "1.0";
    public string NombreArchivo { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    /// <summary>Resumen de cambios de la versión.</summary>
    public string Nota { get; set; } = string.Empty;
    public Guid SubidoPorId { get; set; }
    public ApplicationUser SubidoPor { get; set; } = null!;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
