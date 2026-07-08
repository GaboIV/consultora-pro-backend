using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Notificaciones;

/// <summary>
/// Evento de negocio a notificar. En Titulo/Mensaje se puede usar el token {actor},
/// que se sustituye por el nombre del usuario que ejecutó la acción.
/// </summary>
public class PublicarNotificacionDto
{
    public TipoNotificacion Tipo { get; set; }
    public List<Guid> DestinatarioIds { get; set; } = new();
    /// <summary>Usuario que ejecutó la acción. Salvo IncluirActor, se excluye de los destinatarios.</summary>
    public Guid? ActorId { get; set; }
    /// <summary>True para notificaciones de seguridad donde el actor también debe enterarse (ej. su propia contraseña).</summary>
    public bool IncluirActor { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    /// <summary>Ruta relativa del frontend (ej. "/mis-tableros/{id}").</summary>
    public string? Url { get; set; }
    /// <summary>Clave de deduplicación del correo dentro de la ventana de agrupación (solo tipos agrupables).</summary>
    public string? DedupKey { get; set; }
}
