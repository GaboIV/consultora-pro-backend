using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Ambientes;

public class AmbienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoAmbiente Tipo { get; set; }
    public string? Url { get; set; }
    public string? HealthCheckUrl { get; set; }
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string? Tecnologia { get; set; }
    public EstadoAmbiente Estado { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateAmbienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public TipoAmbiente Tipo { get; set; }
    public string? Url { get; set; }
    public string? HealthCheckUrl { get; set; }
    public Guid ProyectoId { get; set; }
    public string? Tecnologia { get; set; }
    public EstadoAmbiente Estado { get; set; } = EstadoAmbiente.Configurando;
}

public class UpdateAmbienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public TipoAmbiente Tipo { get; set; }
    public string? Url { get; set; }
    public string? HealthCheckUrl { get; set; }
    public Guid ProyectoId { get; set; }
    public string? Tecnologia { get; set; }
    public EstadoAmbiente Estado { get; set; }
}

public class UpdateAmbienteEstadoDto
{
    public EstadoAmbiente Estado { get; set; }
}
