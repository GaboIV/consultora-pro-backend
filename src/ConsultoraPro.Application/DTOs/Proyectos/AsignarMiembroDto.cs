using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Proyectos;

public class AsignarMiembroDto
{
    public Guid UsuarioId { get; set; }
    public RolDesarrollador Rol { get; set; }
}
