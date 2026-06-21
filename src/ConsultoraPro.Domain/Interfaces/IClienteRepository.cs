using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> GetAllAsync(Guid? memberUserId = null);
    Task<IEnumerable<Cliente>> GetPagedAsync(int page, int pageSize, string? search = null, Guid? memberUserId = null);
    Task<int> GetTotalCountAsync(string? search = null, Guid? memberUserId = null);
    Task<Cliente?> GetByIdAsync(Guid id);
    Task<Cliente> CreateAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
    Task DeleteAsync(Cliente cliente);
}
