using ConsultoraPro.Application.DTOs.AmbienteComponentes;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AmbienteComponenteService : IAmbienteComponenteService
{
    private readonly IAmbienteComponenteRepository _repository;
    private readonly IAmbienteRepository _ambienteRepository;

    public AmbienteComponenteService(IAmbienteComponenteRepository repository, IAmbienteRepository ambienteRepository)
    {
        _repository = repository;
        _ambienteRepository = ambienteRepository;
    }

    public async Task<IEnumerable<AmbienteComponenteDto>> GetByAmbienteAsync(Guid ambienteId)
    {
        var items = await _repository.GetByAmbienteAsync(ambienteId);
        return items.Where(x => x.Activo).Select(ToDto);
    }

    public async Task<AmbienteComponenteDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null || !entity.Activo ? null : ToDto(entity);
    }

    public async Task<AmbienteComponenteDto> CreateAsync(CreateAmbienteComponenteDto dto)
    {
        await EnsureAmbienteExistsAsync(dto.AmbienteId);

        var entity = new AmbienteComponente
        {
            Id = Guid.NewGuid(),
            AmbienteId = dto.AmbienteId,
            Rol = dto.Rol.Trim(),
            IpPublica = dto.IpPublica?.Trim(),
            IpPrivada = dto.IpPrivada?.Trim(),
            Hostname = dto.Hostname?.Trim(),
            Tecnologia = dto.Tecnologia?.Trim(),
            Especificaciones = dto.Especificaciones?.Trim(),
            Activo = true
        };

        var created = await _repository.CreateAsync(entity);
        return ToDto(created);
    }

    public async Task UpdateAsync(Guid id, UpdateAmbienteComponenteDto dto)
    {
        var entity = await GetActiveEntityAsync(id);

        entity.Rol = dto.Rol.Trim();
        entity.IpPublica = dto.IpPublica?.Trim();
        entity.IpPrivada = dto.IpPrivada?.Trim();
        entity.Hostname = dto.Hostname?.Trim();
        entity.Tecnologia = dto.Tecnologia?.Trim();
        entity.Especificaciones = dto.Especificaciones?.Trim();

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetActiveEntityAsync(id);
        entity.Activo = false;
        await _repository.UpdateAsync(entity);
    }

    private async Task<AmbienteComponente> GetActiveEntityAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null || !entity.Activo)
            throw new KeyNotFoundException($"Componente con ID {id} no encontrado");

        return entity;
    }

    private async Task EnsureAmbienteExistsAsync(Guid ambienteId)
    {
        var ambiente = await _ambienteRepository.GetByIdAsync(ambienteId);
        if (ambiente is null || !ambiente.Activo)
            throw new KeyNotFoundException($"Ambiente con ID {ambienteId} no encontrado");
    }

    private static AmbienteComponenteDto ToDto(AmbienteComponente entity)
    {
        return new AmbienteComponenteDto
        {
            Id = entity.Id,
            AmbienteId = entity.AmbienteId,
            Rol = entity.Rol,
            IpPublica = entity.IpPublica,
            IpPrivada = entity.IpPrivada,
            Hostname = entity.Hostname,
            Tecnologia = entity.Tecnologia,
            Especificaciones = entity.Especificaciones
        };
    }
}
