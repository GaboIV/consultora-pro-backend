using ConsultoraPro.Application.DTOs.AmbienteCloudResources;

namespace ConsultoraPro.Application.Interfaces;

public interface IAmbienteCloudResourceService
{
    Task<IEnumerable<AmbienteCloudResourceDto>> GetByAmbienteAsync(Guid ambienteId);
    Task<AmbienteCloudResourceDto?> GetByIdAsync(Guid id);
    Task<AmbienteCloudResourceDto> CreateAsync(CreateAmbienteCloudResourceDto dto);
    Task UpdateAsync(Guid id, UpdateAmbienteCloudResourceDto dto);
    Task DeleteAsync(Guid id);
}
