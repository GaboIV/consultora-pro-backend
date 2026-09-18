using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Documentos;

/// <summary>Todo lo que necesita la pestaña Documentación de un proyecto, en una sola llamada.</summary>
public class DocumentacionProyectoDto
{
    public List<CarpetaDocumentoDto> Carpetas { get; set; } = new();
    public List<DocumentoDto> Documentos { get; set; } = new();
    /// <summary>Límite por archivo en bytes, para validar en cliente antes de subir.</summary>
    public long MaxBytes { get; set; }
    public List<string> ExtensionesPermitidas { get; set; } = new();
}

public class CarpetaDocumentoDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsSistema { get; set; }
}

public class DocumentoDto
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Guid CarpetaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoDocumento Tipo { get; set; }
    public EstadoDocumento Estado { get; set; }
    public string VersionActual { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    /// <summary>URL firmada de corta expiración para vista previa en el navegador.</summary>
    public string Url { get; set; } = string.Empty;
    public List<string> Etiquetas { get; set; } = new();
    public string CreadoPorNombre { get; set; } = string.Empty;
    public string ActualizadoPorNombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DocumentoVersionDto> Versiones { get; set; } = new();
}

public class DocumentoVersionDto
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public string Version { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string Nota { get; set; } = string.Empty;
    public string SubidoPorNombre { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; }
    public bool EsActual { get; set; }
}

/// <summary>Archivo recibido por la API, desacoplado de IFormFile.</summary>
public sealed record ArchivoEntrante(Stream Contenido, string NombreArchivo, string ContentType, long TamanoBytes);

public class SubirDocumentoCommand
{
    public Guid ProyectoId { get; set; }
    public Guid CarpetaId { get; set; }
    public string? Titulo { get; set; }
    public TipoDocumento Tipo { get; set; } = TipoDocumento.Otro;
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.Borrador;
    public string? Version { get; set; }
    public string? Descripcion { get; set; }
    public List<string> Etiquetas { get; set; } = new();
    public ArchivoEntrante Archivo { get; set; } = null!;
}

public class NuevaVersionCommand
{
    public string? Version { get; set; }
    public string? Nota { get; set; }
    public EstadoDocumento? Estado { get; set; }
    public ArchivoEntrante Archivo { get; set; } = null!;
}

public class ActualizarDocumentoDto
{
    public Guid CarpetaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TipoDocumento Tipo { get; set; }
    public EstadoDocumento Estado { get; set; }
    public List<string> Etiquetas { get; set; } = new();
}

public class CrearCarpetaDto
{
    public Guid ProyectoId { get; set; }
    public Guid? ParentId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class RenombrarCarpetaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public sealed record DescargaDocumento(Stream Contenido, string ContentType, string NombreArchivo);
