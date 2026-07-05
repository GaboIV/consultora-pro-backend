using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Notificaciones;

public class NotificacionDto
{
    public Guid Id { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? ActorNombre { get; set; }
    public string? ActorIniciales { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ResumenNotificacionesDto
{
    public int NoLeidas { get; set; }
}
