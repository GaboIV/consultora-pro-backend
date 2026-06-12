using ConsultoraPro.Application.DTOs.AmbienteComponentes;

namespace ConsultoraPro.Application.Interfaces;

public interface IAmbienteComponenteService
{
    Task<IEnumerable<AmbienteComponenteDto>> GetByAmbienteAsync(Guid ambienteId);
    Task<AmbienteComponenteDto?> GetByIdAsync(Guid id);
    Task<AmbienteComponenteDto> CreateAsync(CreateAmbienteComponenteDto dto);
    Task UpdateAsync(Guid id, UpdateAmbienteComponenteDto dto);
    Task DeleteAsync(Guid id);
}
