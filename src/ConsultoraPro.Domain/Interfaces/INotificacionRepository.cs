using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface INotificacionRepository
{
    // Emisión
    Task AddNotificacionesAsync(IEnumerable<Notificacion> notificaciones);

    /// <summary>
    /// Encola un correo en el outbox. Si <paramref name="correo"/> trae DedupKey y ya existe
    /// una fila Pendiente con la misma clave y usuario, actualiza su contenido (conservando
    /// la fecha programada original) en lugar de insertar otra.
    /// </summary>
    Task EnqueueCorreosAsync(IEnumerable<CorreoPendiente> correos);

    /// <summary>Usuarios activos (con su email) a partir de una lista de ids.</summary>
    Task<IReadOnlyList<ApplicationUser>> GetUsuariosActivosAsync(IEnumerable<Guid> ids);

    /// <summary>Ids de usuarios activos cuyo rol tiene concedido el permiso indicado.</summary>
    Task<IReadOnlyList<Guid>> GetUsuarioIdsConPermisoAsync(string permisoClave);

    // Bandeja del usuario
    Task<(IReadOnlyList<Notificacion> Items, int Total)> GetByUsuarioAsync(
        Guid usuarioId, int page, int pageSize, bool soloNoLeidas);
    Task<int> CountNoLeidasAsync(Guid usuarioId);
    Task<bool> MarcarLeidaAsync(Guid usuarioId, Guid notificacionId);
    Task<int> MarcarTodasLeidasAsync(Guid usuarioId);

    // Preferencias
    Task<IReadOnlyList<PreferenciaNotificacion>> GetPreferenciasAsync(Guid usuarioId);
    Task<IReadOnlyList<PreferenciaNotificacion>> GetPreferenciasAsync(IEnumerable<Guid> usuarioIds, TipoNotificacion tipo);
    Task UpsertPreferenciasAsync(Guid usuarioId, IEnumerable<PreferenciaNotificacion> preferencias);

    // Procesador del outbox
    Task<IReadOnlyList<CorreoPendiente>> GetCorreosParaEnviarAsync(DateTime ahora, int maxIntentos, int lote);
    Task UpdateCorreosAsync(IEnumerable<CorreoPendiente> correos);
}
