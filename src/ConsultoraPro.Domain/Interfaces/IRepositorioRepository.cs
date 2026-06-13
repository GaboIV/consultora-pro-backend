using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IRepositorioRepository
{
    Task<IEnumerable<Repositorio>> GetAllAsync(Guid? proyectoId = null);
    Task<Repositorio?> GetByIdAsync(Guid id);
    Task<Repositorio> CreateAsync(Repositorio repositorio);
    Task UpdateAsync(Repositorio repositorio);
}
