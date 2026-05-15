using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Ambiente
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoAmbiente Tipo { get; set; }
    public string Url { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public string Tecnologia { get; set; } = string.Empty;
    public EstadoAmbiente Estado { get; set; }
    public decimal UptimePorcentaje { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public ICollection<Credencial> Credenciales { get; set; } = new List<Credencial>();
    public ICollection<Despliegue> Despliegues { get; set; } = new List<Despliegue>();
}
