using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Despliegue
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Guid AmbienteId { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public string Version { get; set; } = string.Empty;
    public Guid EjecutadoPorId { get; set; }
    public ApplicationUser EjecutadoPor { get; set; } = null!;
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public EstadoDespliegue Estado { get; set; }
    public int DuracionSegundos { get; set; }
    public string Notas { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
