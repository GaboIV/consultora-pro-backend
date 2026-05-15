using ConsultoraPro.Application.DTOs.Despliegues;

namespace ConsultoraPro.Application.Interfaces;

public interface IDespliegueService
{
    Task<PagedResultDto<DespliegueListDto>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<PagedResultDto<DespliegueListDto>> GetByProjectAsync(Guid proyectoId, int page = 1, int pageSize = 50);
    Task<IEnumerable<DespliegueListDto>> GetRecentAsync(int count);
    Task<DespliegueDto?> GetByIdAsync(Guid id);
    Task<DespliegueDto> CreateAsync(CreateDespliegueDto dto, Guid ejecutadoPorId);
    Task UpdateEstadoAsync(Guid id, UpdateDespliegueEstadoDto dto);
}
