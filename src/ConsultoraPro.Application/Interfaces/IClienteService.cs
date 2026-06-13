using ConsultoraPro.Application.DTOs.Clientes;
using ConsultoraPro.Application.DTOs.Common;

namespace ConsultoraPro.Application.Interfaces;

public interface IClienteService
{
    Task<PagedResultDto<ClienteDto>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<ClienteDto?> GetByIdAsync(Guid id);
    Task<ClienteDto> CreateAsync(CreateClienteDto dto);
    Task UpdateAsync(Guid id, UpdateClienteDto dto);
    Task DeleteAsync(Guid id);
}
