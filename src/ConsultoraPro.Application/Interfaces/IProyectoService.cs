using ConsultoraPro.Application.DTOs.Proyectos;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.Interfaces;

public interface IProyectoService
{
    Task<PagedResultDto<ProyectoDto>> GetAllAsync(int page = 1, int pageSize = 20, EstadoProyecto? estado = null, Guid? clienteId = null);
    Task<ProyectoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProyectoDto>> GetByClienteIdAsync(Guid clienteId);
    Task<ProyectoDto> CreateAsync(CreateProyectoDto dto);
    Task UpdateAsync(Guid id, UpdateProyectoDto dto);
    Task DeleteAsync(Guid id);
}
