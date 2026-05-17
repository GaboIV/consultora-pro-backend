using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IAmbienteTestUserRepository
{
    Task<IEnumerable<AmbienteTestUser>> GetByAmbienteAsync(Guid ambienteId);
    Task<AmbienteTestUser?> GetByIdAsync(Guid id);
    Task<AmbienteTestUser> CreateAsync(AmbienteTestUser entity);
    Task UpdateAsync(AmbienteTestUser entity);
}
