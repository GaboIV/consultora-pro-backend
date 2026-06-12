using ConsultoraPro.Application.DTOs.Ambientes;

namespace ConsultoraPro.Application.Interfaces;

public interface IAmbienteService
{
    Task<IEnumerable<AmbienteDto>> GetAllAsync(Guid? proyectoId = null);
    Task<AmbienteDto?> GetByIdAsync(Guid id);
    Task<AmbienteDto> CreateAsync(CreateAmbienteDto dto);
    Task UpdateAsync(Guid id, UpdateAmbienteDto dto);
    Task UpdateEstadoAsync(Guid id, UpdateAmbienteEstadoDto dto);
    Task DeleteAsync(Guid id);
}
