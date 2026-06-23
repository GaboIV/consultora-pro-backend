using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Interfaces;

public interface IProyectoRepository
{
    Task<IEnumerable<Proyecto>> GetAllAsync(Guid? memberUserId = null);
    Task<IEnumerable<Proyecto>> GetPagedAsync(int page, int pageSize, EstadoProyecto? estado = null, Guid? clienteId = null, Guid? memberUserId = null);
    Task<int> GetTotalCountAsync(EstadoProyecto? estado = null, Guid? clienteId = null, Guid? memberUserId = null);
    Task<Proyecto?> GetByIdAsync(Guid id);
    Task<IEnumerable<Proyecto>> GetByClienteIdAsync(Guid clienteId);
    Task<Proyecto> CreateAsync(Proyecto proyecto);
    Task UpdateAsync(Proyecto proyecto);
    Task AddChildrenAndSaveAsync(params object[] entities);
    Task DeleteAsync(Proyecto proyecto);
}
