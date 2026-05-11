using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Repositorio
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public ProveedorRepositorio Proveedor { get; set; }
    public string RamaPrincipal { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public EstadoPipeline EstadoPipeline { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
