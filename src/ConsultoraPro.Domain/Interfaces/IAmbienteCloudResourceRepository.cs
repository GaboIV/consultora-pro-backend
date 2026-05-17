using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IAmbienteCloudResourceRepository
{
    Task<IEnumerable<AmbienteCloudResource>> GetByAmbienteAsync(Guid ambienteId);
    Task<AmbienteCloudResource?> GetByIdAsync(Guid id);
    Task<AmbienteCloudResource> CreateAsync(AmbienteCloudResource entity);
    Task UpdateAsync(AmbienteCloudResource entity);
}
