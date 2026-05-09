using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IDesarrolladorRepository
{
    Task<IEnumerable<Desarrollador>> GetByProyectoIdAsync(Guid proyectoId);
    Task<Desarrollador?> GetByIdAsync(Guid id);
    Task<Desarrollador> CreateAsync(Desarrollador desarrollador);
    Task UpdateAsync(Desarrollador desarrollador);
    Task DeleteAsync(Desarrollador desarrollador);
}
