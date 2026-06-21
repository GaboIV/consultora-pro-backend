using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface ISolicitudRevelacionRepository
{
    Task<SolicitudRevelacionCredencial?> GetByIdAsync(Guid id);

    /// <summary>Solicitud pendiente del usuario para la credencial, si existe.</summary>
    Task<SolicitudRevelacionCredencial?> GetPendienteAsync(Guid credencialId, Guid solicitanteId);

    /// <summary>Aprobación vigente (no expirada) del usuario para la credencial, si existe.</summary>
    Task<SolicitudRevelacionCredencial?> GetVigenteAsync(Guid credencialId, Guid usuarioId, DateTime now);

    /// <summary>Solicitudes filtradas por estado (todas si <paramref name="estado"/> es null), para la bandeja del aprobador.</summary>
    Task<IEnumerable<SolicitudRevelacionCredencial>> ListAsync(EstadoSolicitudRevelacion? estado);

    /// <summary>Solicitudes creadas por un usuario concreto.</summary>
    Task<IEnumerable<SolicitudRevelacionCredencial>> ListBySolicitanteAsync(Guid solicitanteId);

    Task CreateAsync(SolicitudRevelacionCredencial solicitud);
    Task UpdateAsync(SolicitudRevelacionCredencial solicitud);
}
