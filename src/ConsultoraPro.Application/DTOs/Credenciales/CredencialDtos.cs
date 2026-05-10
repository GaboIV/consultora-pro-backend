using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Credenciales;

public class CredencialListDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public Guid? AmbienteId { get; set; }
    public string? AmbienteNombre { get; set; }
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
    public Guid ProyectoId { get; set; }
    public Guid? AmbienteId { get; set; }
    public string Valor { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
}

public class UpdateCredencialDto
{
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public Guid? AmbienteId { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class UpdateCredencialValorDto
{
    public string Valor { get; set; } = string.Empty;
}

public class CredencialRevealDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public DateTime ReveladoEn { get; set; }
    public int VisiblePorSegundos { get; set; } = 30;
}

public class AuditoriaCredencialDto
{
    public Guid Id { get; set; }
    public Guid CredencialId { get; set; }
    public Guid UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaRevelacion { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
