using ConsultoraPro.Application.DTOs.Repositorios;

namespace ConsultoraPro.Application.Interfaces;

public interface IRepositorioService
{
    Task<IEnumerable<RepositorioDto>> GetAllAsync(Guid? proyectoId = null);
    Task<RepositorioDto?> GetByIdAsync(Guid id);
    Task<RepositorioDto> CreateAsync(CreateRepositorioDto dto);
    Task UpdateAsync(Guid id, UpdateRepositorioDto dto);
    Task DeleteAsync(Guid id);
}
