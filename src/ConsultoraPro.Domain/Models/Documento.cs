using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Documento de proyecto. Guarda los metadatos corporativos (código, tipo, estado, etiquetas) y
/// replica los datos del archivo de la versión vigente para listar y buscar sin joins; el
/// histórico completo de archivos vive en <see cref="Versiones"/>.
/// </summary>
public class Documento : IProyectoScoped
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Guid CarpetaId { get; set; }
    public CarpetaDocumento Carpeta { get; set; } = null!;
    /// <summary>Código corporativo único por proyecto (ej. "CMH-EF-003").</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoDocumento Tipo { get; set; }
    public EstadoDocumento Estado { get; set; }

    // --- Archivo de la versión vigente (denormalizado desde DocumentoVersion). ---
    public string VersionActual { get; set; } = "1.0";
    public string NombreArchivo { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    /// <summary>Key relativa en el almacenamiento de la versión vigente. No es una URL.</summary>
    public string StorageKey { get; set; } = string.Empty;

    public Guid CreadoPorId { get; set; }
    public ApplicationUser CreadoPor { get; set; } = null!;
    public Guid? ActualizadoPorId { get; set; }
    public ApplicationUser? ActualizadoPor { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Borrado lógico: el documento y sus archivos se conservan para auditoría.</summary>
    public bool Activo { get; set; } = true;

    public ICollection<DocumentoVersion> Versiones { get; set; } = new List<DocumentoVersion>();
    public ICollection<DocumentoEtiqueta> Etiquetas { get; set; } = new List<DocumentoEtiqueta>();
}
