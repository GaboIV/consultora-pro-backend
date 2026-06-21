using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IAmbienteRepository
{
    Task<IEnumerable<Ambiente>> GetAllAsync(Guid? proyectoId = null, Guid? memberUserId = null);
    Task<Ambiente?> GetByIdAsync(Guid id);
    Task<Ambiente> CreateAsync(Ambiente ambiente);
    Task UpdateAsync(Ambiente ambiente);
}
