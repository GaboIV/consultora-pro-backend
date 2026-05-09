using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Desarrollador
{
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public RolDesarrollador Rol { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
}
