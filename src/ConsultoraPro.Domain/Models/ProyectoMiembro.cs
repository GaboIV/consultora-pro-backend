using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class ProyectoMiembro
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public RolDesarrollador Rol { get; set; } = RolDesarrollador.Apoyo;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
