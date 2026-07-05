using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Notificacion
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    /// <summary>Ruta relativa del frontend a la que navega la notificación (ej. "/proyectos/{id}").</summary>
    public string? Url { get; set; }
    public Guid? ActorId { get; set; }
    public ApplicationUser? Actor { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLectura { get; set; }
}
