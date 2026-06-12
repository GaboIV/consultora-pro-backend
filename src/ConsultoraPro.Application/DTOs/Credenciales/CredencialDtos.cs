using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Credenciales;

public class CredencialListDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Puerto { get; set; }
    public string? Usuario { get; set; }
    public string? Url { get; set; }
    public string? Notas { get; set; }
    public Dictionary<string, string>? CamposExtra { get; set; }
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public Guid? AmbienteId { get; set; }
    public string? AmbienteNombre { get; set; }
    public string? AmbienteTipo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
    public string EstadoVencimiento { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CredencialDetalleDto : CredencialListDto
{
    public Guid CreadoPor { get; set; }
    public string CreadoPorNombre { get; set; } = string.Empty;
}

public class CreateCredencialDto
{
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Puerto { get; set; }
    public string? Usuario { get; set; }
    public string? Url { get; set; }
    public string? Notas { get; set; }
    public Dictionary<string, string>? CamposExtra { get; set; }
    public Guid ProyectoId { get; set; }
    public Guid? AmbienteId { get; set; }
    public string Valor { get; set; } = string.Empty;
    public Dictionary<string, string>? SecretosExtra { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class UpdateCredencialDto
{
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Puerto { get; set; }
    public string? Usuario { get; set; }
    public string? Url { get; set; }
    public string? Notas { get; set; }
    public Dictionary<string, string>? CamposExtra { get; set; }
    public Guid ProyectoId { get; set; }
    public Guid? AmbienteId { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class UpdateCredencialValorDto
{
    public string Valor { get; set; } = string.Empty;
    public Dictionary<string, string>? SecretosExtra { get; set; }
}

public class CredencialRevealDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public Dictionary<string, string>? SecretosExtra { get; set; }
    public DateTime ReveladoEn { get; set; }
    public int VisiblePorSegundos { get; set; } = 30;
}

public class RegistrarCopiadoDto
{
    /// <summary>Campo copiado (host, usuario, secreto, cadena…), solo informativo para la auditoría.</summary>
    public string? Campo { get; set; }
}

public class AuditoriaCredencialDto
{
    public Guid Id { get; set; }
    public Guid CredencialId { get; set; }
    public Guid UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Accion { get; set; } = "Lectura";
    public string? Detalle { get; set; }
    public DateTime FechaRevelacion { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class ImportCredencialRowDto : CreateCredencialDto
{
    /// <summary>Número de fila del archivo origen, para reportar errores al usuario.</summary>
    public int Fila { get; set; }
}

public class ImportCredencialesDto
{
    public List<ImportCredencialRowDto> Filas { get; set; } = new();
}

public class ImportRowErrorDto
{
    public int Fila { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class ImportResultDto
{
    public int Total { get; set; }
    public int Importadas { get; set; }
    public List<ImportRowErrorDto> Errores { get; set; } = new();
}
