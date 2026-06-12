using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.DTOs.Common;

namespace ConsultoraPro.Application.Interfaces;

public interface ICredencialService
{
    Task<PagedResultDto<CredencialListDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? proyectoId = null);
    Task<CredencialDetalleDto?> GetByIdAsync(Guid id);
    Task<CredencialListDto> CreateAsync(CreateCredencialDto dto, Guid userId);
    Task UpdateAsync(Guid id, UpdateCredencialDto dto);
    Task UpdateValorAsync(Guid id, UpdateCredencialValorDto dto);
    Task DeleteAsync(Guid id);
    Task<CredencialRevealDto> RevealAsync(Guid id, Guid userId, string ip, string userAgent);
    Task RegistrarCopiadoAsync(Guid id, Guid userId, string ip, string userAgent, string? campo);
    Task<ImportResultDto> ImportAsync(ImportCredencialesDto dto, Guid userId);
    Task<IEnumerable<AuditoriaCredencialDto>> GetAuditAsync(Guid credencialId);
}
