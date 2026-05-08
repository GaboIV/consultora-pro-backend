using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Proyecto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public EtapaProyecto Etapa { get; set; }
    public EstadoProyecto Estado { get; set; }
    public int Progreso { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string TechLead { get; set; } = string.Empty;
    public string TechLeadIniciales { get; set; } = string.Empty;
    public int TotalMiembros { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
