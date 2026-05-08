using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IProyectoRepository
{
    Task<IEnumerable<Proyecto>> GetAllAsync();
    Task<Proyecto?> GetByIdAsync(Guid id);
    Task<IEnumerable<Proyecto>> GetByClienteIdAsync(Guid clienteId);
    Task<Proyecto> CreateAsync(Proyecto proyecto);
    Task UpdateAsync(Proyecto proyecto);
    Task DeleteAsync(Proyecto proyecto);
}
