using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Preferencia por usuario y tipo. Si no existe fila para un tipo, aplican los
/// defaults de <see cref="Notificaciones.NotificacionCatalog"/>.
/// </summary>
public class PreferenciaNotificacion
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public TipoNotificacion Tipo { get; set; }
    public bool EnApp { get; set; } = true;
    public bool PorCorreo { get; set; } = true;
}
