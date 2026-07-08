using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Notificaciones;

namespace ConsultoraPro.Application.Interfaces;

public interface INotificacionService
{
    /// <summary>
    /// Punto único de emisión. Resuelve preferencias por destinatario, crea las notificaciones
    /// en la campana y encola los correos en el outbox. Nunca lanza: un fallo al notificar no
    /// debe romper la operación de negocio que lo originó.
    /// </summary>
    Task PublicarAsync(PublicarNotificacionDto evento);

    Task<PagedResultDto<NotificacionDto>> GetMisNotificacionesAsync(Guid usuarioId, int page, int pageSize, bool soloNoLeidas);
    Task<ResumenNotificacionesDto> GetResumenAsync(Guid usuarioId);
    Task<bool> MarcarLeidaAsync(Guid usuarioId, Guid notificacionId);
    Task<int> MarcarTodasLeidasAsync(Guid usuarioId);

    Task<IReadOnlyList<PreferenciaNotificacionDto>> GetPreferenciasAsync(Guid usuarioId);
    Task ActualizarPreferenciasAsync(Guid usuarioId, UpdatePreferenciasNotificacionDto dto);
}
