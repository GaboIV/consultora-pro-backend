using ConsultoraPro.Application.DTOs.AmbienteCloudResources;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AmbienteCloudResourceService : IAmbienteCloudResourceService
{
    private readonly IAmbienteCloudResourceRepository _repository;
    private readonly IAmbienteRepository _ambienteRepository;

    public AmbienteCloudResourceService(IAmbienteCloudResourceRepository repository, IAmbienteRepository ambienteRepository)
    {
        _repository = repository;
        _ambienteRepository = ambienteRepository;
    }

    public async Task<IEnumerable<AmbienteCloudResourceDto>> GetByAmbienteAsync(Guid ambienteId)
    {
        var items = await _repository.GetByAmbienteAsync(ambienteId);
        return items.Where(x => x.Activo).Select(ToDto);
    }

    public async Task<AmbienteCloudResourceDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null || !entity.Activo ? null : ToDto(entity);
    }

    public async Task<AmbienteCloudResourceDto> CreateAsync(CreateAmbienteCloudResourceDto dto)
    {
        await EnsureAmbienteExistsAsync(dto.AmbienteId);

        var entity = new AmbienteCloudResource
        {
            Id = Guid.NewGuid(),
            AmbienteId = dto.AmbienteId,
            TipoRecurso = dto.TipoRecurso.Trim(),
            NombreRecurso = dto.NombreRecurso.Trim(),
            DeepLink = dto.DeepLink?.Trim(),
            Activo = true
        };

        var created = await _repository.CreateAsync(entity);
        return ToDto(created);
    }

    public async Task UpdateAsync(Guid id, UpdateAmbienteCloudResourceDto dto)
    {
        var entity = await GetActiveEntityAsync(id);

        entity.TipoRecurso = dto.TipoRecurso.Trim();
        entity.NombreRecurso = dto.NombreRecurso.Trim();
        entity.DeepLink = dto.DeepLink?.Trim();

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetActiveEntityAsync(id);
        entity.Activo = false;
        await _repository.UpdateAsync(entity);
    }

    private async Task<AmbienteCloudResource> GetActiveEntityAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null || !entity.Activo)
            throw new KeyNotFoundException($"Recurso en la nube con ID {id} no encontrado");

        return entity;
    }

    private async Task EnsureAmbienteExistsAsync(Guid ambienteId)
    {
        var ambiente = await _ambienteRepository.GetByIdAsync(ambienteId);
        if (ambiente is null || !ambiente.Activo)
            throw new KeyNotFoundException($"Ambiente con ID {ambienteId} no encontrado");
    }

    private static AmbienteCloudResourceDto ToDto(AmbienteCloudResource entity)
    {
        return new AmbienteCloudResourceDto
        {
            Id = entity.Id,
            AmbienteId = entity.AmbienteId,
            TipoRecurso = entity.TipoRecurso,
            NombreRecurso = entity.NombreRecurso,
            DeepLink = entity.DeepLink
        };
    }
}
