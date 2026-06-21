using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.Interfaces;

public interface ICredencialService
{
    Task<PagedResultDto<CredencialListDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? proyectoId = null);
    Task<CredencialDetalleDto?> GetByIdAsync(Guid id);
    Task<CredencialListDto> CreateAsync(CreateCredencialDto dto, Guid userId);
    Task UpdateAsync(Guid id, UpdateCredencialDto dto);
    Task UpdateValorAsync(Guid id, UpdateCredencialValorDto dto);
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Revela los secretos. Si <paramref name="puedeRevelarDirecto"/> es false (nivel básico),
    /// exige una aprobación de revelación vigente; de lo contrario lanza
    /// <see cref="Exceptions.RevelacionRequiereSolicitudException"/>.
    /// </summary>
    Task<CredencialRevealDto> RevealAsync(Guid id, Guid userId, bool puedeRevelarDirecto, string ip, string userAgent);
    Task RegistrarCopiadoAsync(Guid id, Guid userId, string ip, string userAgent, string? campo);
    Task<ImportResultDto> ImportAsync(ImportCredencialesDto dto, Guid userId);
    Task<IEnumerable<AuditoriaCredencialDto>> GetAuditAsync(Guid credencialId);

    // --- Flujo de solicitud de revelación (nivel básico → ver-todo temporal). ---
    Task<SolicitudRevelacionDto> CrearSolicitudAsync(Guid credencialId, Guid solicitanteId, string? motivo);
    Task<IEnumerable<SolicitudRevelacionDto>> GetSolicitudesAsync(EstadoSolicitudRevelacion? estado);
    Task<IEnumerable<SolicitudRevelacionDto>> GetMisSolicitudesAsync(Guid solicitanteId);
    Task<SolicitudRevelacionDto> ResolverSolicitudAsync(Guid solicitudId, Guid aprobadorId, bool aprobar, string? nota);
}
