using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Interfaces;

public interface IProyectoRepository
{
    Task<IEnumerable<Proyecto>> GetAllAsync();
    Task<IEnumerable<Proyecto>> GetPagedAsync(int page, int pageSize, EstadoProyecto? estado = null, Guid? clienteId = null);
    Task<int> GetTotalCountAsync(EstadoProyecto? estado = null, Guid? clienteId = null);
    Task<Proyecto?> GetByIdAsync(Guid id);
    Task<IEnumerable<Proyecto>> GetByClienteIdAsync(Guid clienteId);
    Task<Proyecto> CreateAsync(Proyecto proyecto);
    Task UpdateAsync(Proyecto proyecto);
    Task DeleteAsync(Proyecto proyecto);
}
