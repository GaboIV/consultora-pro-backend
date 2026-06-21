using ConsultoraPro.Application.DTOs.Ambientes;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AmbienteService : IAmbienteService
{
    private readonly IAmbienteRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ICurrentUserService _currentUserService;

    public AmbienteService(
        IAmbienteRepository repository, 
        IProyectoRepository proyectoRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AmbienteDto>> GetAllAsync(Guid? proyectoId = null)
    {
        Guid? memberUserId = null;
        if (!_currentUserService.HasFullProjectAccess)
        {
            memberUserId = _currentUserService.UserId;
        }

        var ambientes = await _repository.GetAllAsync(proyectoId, memberUserId);
        return ambientes.Select(ToDto);
    }

    public async Task<AmbienteDto?> GetByIdAsync(Guid id)
    {
        var ambiente = await _repository.GetByIdAsync(id);
        if (ambiente is null || !ambiente.Activo) return null;

        if (!_currentUserService.HasFullProjectAccess)
        {
            var isMember = ambiente.Proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
            if (!isMember) return null;
        }

        return ToDto(ambiente);
    }

    public async Task<AmbienteDto> CreateAsync(CreateAmbienteDto dto)
    {
        if (!_currentUserService.HasFullProjectAccess)
        {
            var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId);
            var isMember = proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
            if (!isMember) throw new UnauthorizedAccessException("No tienes acceso a este proyecto.");
        }

        await EnsureProjectExistsAsync(dto.ProyectoId);

        var ambiente = new Ambiente
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo,
            Url = dto.Url?.Trim(),
            HealthCheckUrl = dto.HealthCheckUrl?.Trim(),
            ProyectoId = dto.ProyectoId,
            Tecnologia = dto.Tecnologia?.Trim(),
            Estado = dto.Estado,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(ambiente);
        var reloaded = await _repository.GetByIdAsync(created.Id);
        return ToDto(reloaded ?? created);
    }

    public async Task UpdateAsync(Guid id, UpdateAmbienteDto dto)
    {
        var ambiente = await GetActiveEntityAsync(id);

        if (!_currentUserService.HasFullProjectAccess)
        {
            var isMember = ambiente.Proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
            if (!isMember) throw new UnauthorizedAccessException("No tienes acceso a este ambiente.");
        }

        await EnsureProjectExistsAsync(dto.ProyectoId);

        ambiente.Nombre = dto.Nombre.Trim();
        ambiente.Tipo = dto.Tipo;
        ambiente.Url = dto.Url?.Trim();
        ambiente.HealthCheckUrl = dto.HealthCheckUrl?.Trim();
        ambiente.ProyectoId = dto.ProyectoId;
        ambiente.Tecnologia = dto.Tecnologia?.Trim();
        ambiente.Estado = dto.Estado;

        await _repository.UpdateAsync(ambiente);
    }

    public async Task UpdateEstadoAsync(Guid id, UpdateAmbienteEstadoDto dto)
    {
        var ambiente = await GetActiveEntityAsync(id);

        if (!_currentUserService.HasFullProjectAccess)
        {
            var isMember = ambiente.Proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
            if (!isMember) throw new UnauthorizedAccessException("No tienes acceso a este ambiente.");
        }

        ambiente.Estado = dto.Estado;
        await _repository.UpdateAsync(ambiente);
    }

    public async Task DeleteAsync(Guid id)
    {
        var ambiente = await GetActiveEntityAsync(id);

        if (!_currentUserService.HasFullProjectAccess)
        {
            var isMember = ambiente.Proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
            if (!isMember) throw new UnauthorizedAccessException("No tienes acceso a este ambiente.");
        }

        ambiente.Activo = false;
        await _repository.UpdateAsync(ambiente);
    }

    private async Task<Ambiente> GetActiveEntityAsync(Guid id)
    {
        var ambiente = await _repository.GetByIdAsync(id);
        if (ambiente is null || !ambiente.Activo)
            throw new KeyNotFoundException($"Ambiente con ID {id} no encontrado");

        return ambiente;
    }

    private async Task EnsureProjectExistsAsync(Guid proyectoId)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
        if (proyecto is null)
            throw new KeyNotFoundException($"Proyecto con ID {proyectoId} no encontrado");
    }

    private static AmbienteDto ToDto(Ambiente ambiente)
    {
        return new AmbienteDto
        {
            Id = ambiente.Id,
            Nombre = ambiente.Nombre,
            Tipo = ambiente.Tipo,
            Url = ambiente.Url,
            HealthCheckUrl = ambiente.HealthCheckUrl,
            ProyectoId = ambiente.ProyectoId,
            ProyectoNombre = ambiente.Proyecto?.Nombre ?? string.Empty,
            ClienteNombre = ambiente.Proyecto?.Cliente?.Nombre ?? string.Empty,
            Tecnologia = ambiente.Tecnologia,
            Estado = ambiente.Estado,
            Activo = ambiente.Activo,
            FechaCreacion = ambiente.FechaCreacion
        };
    }
}
