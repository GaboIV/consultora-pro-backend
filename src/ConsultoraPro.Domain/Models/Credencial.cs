using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Credencial
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Guid? AmbienteId { get; set; }
    public Ambiente? Ambiente { get; set; }
    public string ValorCifrado { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public Guid CreadoPor { get; set; }
    public ApplicationUser Creador { get; set; } = null!;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
    public ICollection<AuditoriaCredencial> Auditorias { get; set; } = new List<AuditoriaCredencial>();
}
