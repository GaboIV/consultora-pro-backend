using ConsultoraPro.Application.DTOs.Kanban;

namespace ConsultoraPro.Application.Interfaces;

public interface IColumnaService
{
    Task<ColumnaDto> CreateAsync(CreateColumnaDto dto);
    Task UpdateAsync(Guid id, UpdateColumnaDto dto);
    Task ReordenarAsync(Guid id, ReordenarColumnaDto dto);
    Task DeleteAsync(Guid id);
}
