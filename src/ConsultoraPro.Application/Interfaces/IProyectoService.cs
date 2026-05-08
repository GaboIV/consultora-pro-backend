using ConsultoraPro.Application.DTOs.Proyectos;

namespace ConsultoraPro.Application.Interfaces;

public interface IProyectoService
{
    Task<IEnumerable<ProyectoDto>> GetAllAsync();
    Task<ProyectoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProyectoDto>> GetByClienteIdAsync(Guid clienteId);
    Task<ProyectoDto> CreateAsync(CreateProyectoDto dto);
    Task UpdateAsync(Guid id, UpdateProyectoDto dto);
    Task DeleteAsync(Guid id);
}
