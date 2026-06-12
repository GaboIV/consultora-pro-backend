using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Proyectos;

public class ProyectoMiembroDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
