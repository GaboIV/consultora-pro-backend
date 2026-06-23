using ConsultoraPro.Application.DTOs.TiposSolucion;

namespace ConsultoraPro.Application.Interfaces;

public interface ITipoSolucionService
{
    Task<List<TipoSolucionDto>> GetAllAsync();
    Task<TipoSolucionDto?> GetByIdAsync(Guid id);
    Task<TipoSolucionDto> CreateAsync(CreateTipoSolucionDto dto);
    Task UpdateAsync(Guid id, UpdateTipoSolucionDto dto);
    Task DeleteAsync(Guid id);
}
