using ConsultoraPro.Application.DTOs.Organigramas;

namespace ConsultoraPro.Application.Interfaces;

public interface IOrganigramaService
{
    Task<List<OrganigramaResumenDto>> GetAllAsync();
    Task<OrganigramaDto?> GetByIdAsync(Guid id);
    Task<List<OrganigramaUsuarioDto>> GetUsuariosDisponiblesAsync();
    Task<OrganigramaDto> CreateAsync(GuardarOrganigramaDto dto);
    Task<OrganigramaDto> UpdateAsync(Guid id, GuardarOrganigramaDto dto);
    Task DeleteAsync(Guid id);
}
