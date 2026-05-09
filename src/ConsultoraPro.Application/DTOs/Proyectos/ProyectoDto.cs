using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Proyectos;

public class ProyectoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid TipoSolucionId { get; set; }
    public string TipoSolucionNombre { get; set; } = string.Empty;
    public string Etapa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int Progreso { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string TechLead { get; set; } = string.Empty;
    public string TechLeadIniciales { get; set; } = string.Empty;
    public int TotalMiembros { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
