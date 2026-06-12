using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IAmbienteComponenteRepository
{
    Task<IEnumerable<AmbienteComponente>> GetByAmbienteAsync(Guid ambienteId);
    Task<AmbienteComponente?> GetByIdAsync(Guid id);
    Task<AmbienteComponente> CreateAsync(AmbienteComponente entity);
    Task UpdateAsync(AmbienteComponente entity);
}
