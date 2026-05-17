using ConsultoraPro.Application.DTOs.AmbienteTestUsers;

namespace ConsultoraPro.Application.Interfaces;

public interface IAmbienteTestUserService
{
    Task<IEnumerable<AmbienteTestUserDto>> GetByAmbienteAsync(Guid ambienteId);
    Task<AmbienteTestUserDto?> GetByIdAsync(Guid id);
    Task<AmbienteTestUserDto> CreateAsync(CreateAmbienteTestUserDto dto);
    Task UpdateAsync(Guid id, UpdateAmbienteTestUserDto dto);
    Task DeleteAsync(Guid id);
}
