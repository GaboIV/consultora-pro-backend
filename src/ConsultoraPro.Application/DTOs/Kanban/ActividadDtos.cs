using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Kanban;

public class ActividadDto
{
    public Guid Id { get; set; }
    public TipoActividadTarjeta Tipo { get; set; }
    public string? Detalle { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public DateTime Fecha { get; set; }
}
