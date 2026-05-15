using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Despliegues;

public class DespliegueDto
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid AmbienteId { get; set; }
    public string AmbienteNombre { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Guid EjecutadoPorId { get; set; }
    public string EjecutadoPorNombre { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public EstadoDespliegue Estado { get; set; }
    public int DuracionSegundos { get; set; }
    public string Notas { get; set; } = string.Empty;
}

public class DespliegueListDto
{
    public Guid Id { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string AmbienteNombre { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string EjecutadoPorNombre { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public EstadoDespliegue Estado { get; set; }
    public int DuracionSegundos { get; set; }
}

public class CreateDespliegueDto
{
    public Guid ProyectoId { get; set; }
    public Guid AmbienteId { get; set; }
    public string Version { get; set; } = string.Empty;
    public int DuracionSegundos { get; set; }
    public string Notas { get; set; } = string.Empty;
}

public class UpdateDespliegueEstadoDto
{
    public EstadoDespliegue Estado { get; set; }
    public int? DuracionSegundos { get; set; }
}

public class PagedResultDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
