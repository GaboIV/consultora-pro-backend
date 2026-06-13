using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Proyectos;

public class CreateDesarrolladorDto
{
    public Guid MemberId { get; set; }
    public RolDesarrollador Rol { get; set; }
}
