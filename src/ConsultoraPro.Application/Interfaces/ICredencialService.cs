using ConsultoraPro.Application.DTOs.Credenciales;

namespace ConsultoraPro.Application.Interfaces;

public interface ICredencialService
{
    Task<IEnumerable<CredencialListDto>> GetAllAsync(Guid? proyectoId = null);
    Task<CredencialDetalleDto?> GetByIdAsync(Guid id);
    Task<CredencialListDto> CreateAsync(CreateCredencialDto dto, Guid userId);
    Task UpdateAsync(Guid id, UpdateCredencialDto dto);
    Task UpdateValorAsync(Guid id, UpdateCredencialValorDto dto);
    Task DeleteAsync(Guid id);
    Task<CredencialRevealDto> RevealAsync(Guid id, Guid userId, string ip, string userAgent);
    Task<IEnumerable<AuditoriaCredencialDto>> GetAuditAsync(Guid credencialId);
}
